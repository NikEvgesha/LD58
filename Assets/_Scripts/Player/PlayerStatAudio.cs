using UnityEngine;

public class PlayerStatAudio : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Если оставить пустым — возьмётся из G.PlayerStatManager")]
    [SerializeField] private PlayerStatManager _stats;

    [Tooltip("Разовые SFX (реакция на урон, замерзание)")]
    [SerializeField] private AudioSource _sfxSource;

    [Tooltip("Источник для удара сердца (проигрываем one-shot по таймеру)")]
    [SerializeField] private AudioSource _heartbeatSource;

    [Tooltip("Источник для дыхания (проигрываем one-shot по таймеру)")]
    [SerializeField] private AudioSource _breathingSource;

    [Header("Клипы")]
    [Tooltip("Набор коротких реакций на урон")]
    [SerializeField] private AudioClip[] _damageReactionClips;

    [Tooltip("Одиночный клип удара сердца («тук»), НЕ луп")]
    [SerializeField] private AudioClip _heartbeatClip;

    [Tooltip("Одиночный клип дыхания (вдох/выдох), НЕ луп")]
    [SerializeField] private AudioClip _breathingClip;

    [Tooltip("Клип «замерзания» при резком скачке страха")]
    [SerializeField] private AudioClip _freezeClip;

    [Header("Нормализация (ссылочные максимумы)")]
    [Tooltip("Макс. HP для расчёта темпа/громкости сердца (должен совпадать с _hpMax в PlayerStatManager)")]
    [SerializeField] private float _hpMaxRef = 100f;

    [Tooltip("Лимит страха для расчёта дыхания (должен совпадать с _fearLimit в PlayerStatManager)")]
    [SerializeField] private float _fearLimitRef = 100f;

    [Header("Сердцебиение (зависит от HP)")]
    [Tooltip("X = HP/_hpMaxRef, Y = BPM")]
    [SerializeField] private AnimationCurve _heartRateByHp = AnimationCurve.Linear(0f, 140f, 1f, 55f);

    [Tooltip("X = HP/_hpMaxRef, Y = громкость 0..1")]
    [SerializeField]
    private AnimationCurve _heartVolByHp = new AnimationCurve(
        new Keyframe(0f, 1f), new Keyframe(1f, 0.15f));

    [Header("Дыхание (зависит от Fear)")]
    [Tooltip("X = Fear/_fearLimitRef, Y = BPM дыхания")]
    [SerializeField] private AnimationCurve _breathRateByFear = AnimationCurve.Linear(0f, 8f, 1f, 22f);

    [Tooltip("X = Fear/_fearLimitRef, Y = громкость 0..1")]
    [SerializeField]
    private AnimationCurve _breathVolByFear = new AnimationCurve(
        new Keyframe(0f, 0.15f), new Keyframe(1f, 1f));

    [Header("Пороги и сглаживание")]
    [Tooltip("Порог разового скачка страха для звука «замерзания» (у тебя в менеджере визуальные эффекты при > 5)")]
    [SerializeField] private float _fearSpikeThreshold = 5f;

    [Tooltip("Анти-спам задержка для реакций на урон")]
    [SerializeField] private float _damageReactionCooldown = 0.2f;

    [Tooltip("Скорость сглаживания изменения громкости/питча")]
    [SerializeField] private float _smoothingSpeed = 8f;

    [Header("3D-звук (опционально)")]
    [Range(0f, 1f)]
    [SerializeField] private float _spatialBlend = 0f;
    [SerializeField] private float _minDistance = 1f;
    [SerializeField] private float _maxDistance = 15f;

    // --- внутреннее состояние ---
    private float _lastHp;
    private float _lastFear;

    private float _heartTimer;
    private float _breathTimer;

    private float _currHeartVol;
    private float _currHeartPitch = 1f;

    private float _currBreathVol;
    private float _currBreathPitch = 1f;

    private float _lastDamageReactTime;

    private void Reset()
    {
        if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();
        if (_heartbeatSource == null) _heartbeatSource = gameObject.AddComponent<AudioSource>();
        if (_breathingSource == null) _breathingSource = gameObject.AddComponent<AudioSource>();

        SetupSource(_sfxSource);
        SetupSource(_heartbeatSource);
        SetupSource(_breathingSource);
    }

    private void Awake()
    {
        if (_stats == null) _stats = G.PlayerStatManager;
        Apply3D(_sfxSource);
        Apply3D(_heartbeatSource);
        Apply3D(_breathingSource);
    }

    private void OnEnable()
    {
        if (_stats == null) return;

        _lastHp = _stats.HP;
        _lastFear = _stats.Fear;

        _stats.ChangeHP.AddListener(OnHpChanged);
        _stats.ChangeFear.AddListener(OnFearChanged);
    }

    private void OnDisable()
    {
        if (_stats == null) return;

        _stats.ChangeHP.RemoveListener(OnHpChanged);
        _stats.ChangeFear.RemoveListener(OnFearChanged);
    }

    private void Update()
    {
        if (_stats == null) return;

        // --- Сердцебиение (HP) ---
        float hpRatio = Mathf.InverseLerp(0f, Mathf.Max(1f, _hpMaxRef), Mathf.Clamp(_stats.HP, 0f, _hpMaxRef));
        float heartBpm = Mathf.Max(1f, _heartRateByHp.Evaluate(hpRatio));
        float heartInterval = 60f / heartBpm;

        float targetHeartVol = Mathf.Clamp01(_heartVolByHp.Evaluate(hpRatio));
        _currHeartVol = Mathf.Lerp(_currHeartVol, targetHeartVol, Time.deltaTime * _smoothingSpeed);

        float targetHeartPitch = Mathf.Lerp(0.9f, 1.2f, Mathf.InverseLerp(60f, 140f, heartBpm));
        _currHeartPitch = Mathf.Lerp(_currHeartPitch, targetHeartPitch, Time.deltaTime * _smoothingSpeed);
        _heartbeatSource.pitch = _currHeartPitch;

        _heartTimer += Time.deltaTime;
        if (_heartTimer >= heartInterval)
        {
            _heartTimer -= heartInterval;
            if (_heartbeatClip != null)
                _heartbeatSource.PlayOneShot(_heartbeatClip, _currHeartVol);
        }

        // --- Дыхание (Fear) ---
        float fearRatio = Mathf.InverseLerp(0f, Mathf.Max(1f, _fearLimitRef), Mathf.Clamp(_stats.Fear, 0f, _fearLimitRef));
        float breathBpm = Mathf.Max(1f, _breathRateByFear.Evaluate(fearRatio));
        float breathInterval = 60f / breathBpm;

        float targetBreathVol = Mathf.Clamp01(_breathVolByFear.Evaluate(fearRatio));
        _currBreathVol = Mathf.Lerp(_currBreathVol, targetBreathVol, Time.deltaTime * _smoothingSpeed);

        float targetBreathPitch = Mathf.Lerp(0.95f, 1.15f, Mathf.InverseLerp(8f, 22f, breathBpm));
        _currBreathPitch = Mathf.Lerp(_currBreathPitch, targetBreathPitch, Time.deltaTime * _smoothingSpeed);
        _breathingSource.pitch = _currBreathPitch;

        _breathTimer += Time.deltaTime;
        if (_breathTimer >= breathInterval)
        {
            _breathTimer -= breathInterval;
            if (_breathingClip != null)
                _breathingSource.PlayOneShot(_breathingClip, _currBreathVol);
        }
    }

    // --- события от PlayerStatManager ---
    private void OnHpChanged(float newHp)
    {
        if (newHp < _lastHp && _damageReactionClips != null && _damageReactionClips.Length > 0)
        {
            if (Time.time - _lastDamageReactTime >= _damageReactionCooldown)
            {
                int idx = Random.Range(0, _damageReactionClips.Length);
                AudioClip clip = _damageReactionClips[idx];
                if (clip != null) _sfxSource.PlayOneShot(clip, 1f);
                _lastDamageReactTime = Time.time;
            }
        }
        _lastHp = newHp;
    }

    private void OnFearChanged(float newFear)
    {
        float delta = newFear - _lastFear;
        if (delta >= _fearSpikeThreshold && _freezeClip != null)
        {
            _sfxSource.PlayOneShot(_freezeClip, 1f);
        }
        _lastFear = newFear;
    }

    // --- вспомогательные ---
    private void SetupSource(AudioSource src)
    {
        src.playOnAwake = false;
        src.loop = false;
        Apply3D(src);
    }

    private void Apply3D(AudioSource src)
    {
        if (src == null) return;
        src.spatialBlend = _spatialBlend;
        src.minDistance = _minDistance;
        src.maxDistance = _maxDistance;
    }

    private void OnValidate()
    {
        Apply3D(_sfxSource);
        Apply3D(_heartbeatSource);
        Apply3D(_breathingSource);
    }
}
