using System.Collections;
using UnityEngine;

/// <summary>
/// Ловушка "шипы": поднимаются при наступании игрока, наносят урон и опускаются.
/// Требует: коллайдер-триггер (зона активации), объект шипов (будем двигать/анимировать).
/// </summary>
public class SpikesTrap : DungeonTrap
{
    [Header("Refs")]
    [Tooltip("Зона активации (trigger). Если не задано, возьмём первый Collider с isTrigger=true на этом объекте или детях.")]
    [SerializeField] private Collider _activationTrigger;

    [Tooltip("Корневой объект шипов, который будет подниматься/опускаться.")]
    [SerializeField] private Transform _spikesRoot;

    [Header("Animation")]
    [Tooltip("Локальное положение шипов в опущенном состоянии.")]
    [SerializeField] private Vector3 _downLocalPos = new Vector3(0, 0, 0);

    [Tooltip("Локальное положение шипов во взведённом состоянии.")]
    [SerializeField] private Vector3 _upLocalPos = new Vector3(0, 1.2f, 0);

    [Tooltip("Время подъёма шипов.")]
    [SerializeField, Min(0.01f)] private float _riseTime = 0.15f;

    [Tooltip("Время удержания в верхнем положении (перед спуском).")]
    [SerializeField, Min(0f)] private float _holdTime = 0.25f;

    [Tooltip("Время спуска шипов.")]
    [SerializeField, Min(0.01f)] private float _fallTime = 0.2f;

    [Tooltip("Кривая для подъёма/спуска (0..1). Опционально.")]
    [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Gameplay")]
    [Tooltip("Слои, которые могут активировать ловушку (например, только Player).")]
    [SerializeField] private LayerMask _activatorLayers = ~0;

    [Tooltip("Наносимый урон при выстреле шипов.")]
    [SerializeField, Min(0)] private int _damage = 10;

    [Tooltip("Время перезарядки после срабатывания, пока ловушка не может сработать снова.")]
    [SerializeField, Min(0f)] private float _cooldown = 1.25f;

    [Tooltip("Одноразовая ли ловушка: после первого срабатывания выключается навсегда.")]
    [SerializeField] private bool _singleUse = false;

    [Tooltip("Наносить урон только в момент 'выстрела' (подъёма). Если false — наносим урон всем, кто стоит на шипах, и в момент подъёма, и пока они наверху (один раз за цикл).")]
    [SerializeField] private bool _damageOnlyOnPop = true;

    [Header("FX (опционально)")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _sfxPop;
    [SerializeField] private AudioClip _sfxRetract;
    [SerializeField] private ParticleSystem _vfxDust;

    // --- runtime ---
    private bool _armed = false;
    private bool _busy = false;
    private float _lastFireTime = -999f;


    private void Reset()
    {
        // Попытка автоматически проставить референсы
        if (_activationTrigger == null)
        {
            _activationTrigger = GetComponentInChildren<Collider>(includeInactive: true);
            if (_activationTrigger != null) _activationTrigger.isTrigger = true;
        }
        if (_spikesRoot == null) _spikesRoot = transform;
    }

    private void Awake()
    {
        if (_spikesRoot == null) _spikesRoot = transform;
        // приводим в "down" на старте
        _spikesRoot.localPosition = _downLocalPos;

        if (_activationTrigger == null)
        {
            _activationTrigger = GetComponentInChildren<Collider>(includeInactive: true);
            if (_activationTrigger != null) _activationTrigger.isTrigger = true;
        }
    }

    /// <summary>
    /// Вызывается твоим генератором при установке тайла. Здесь мы "взводим" ловушку.
    /// </summary>
    public override void Activate()
    {
        _armed = true;
        _busy = false;
        _spikesRoot.localPosition = _downLocalPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_armed || _busy || _activationTrigger == null) return;

        // проверяем, что это наш слой-активатор
        if (((1 << other.gameObject.layer) & _activatorLayers) == 0) return;

        // проверяем, что событие относится к нашему триггеру (если их несколько на объекте)
        // (обычно это не требуется, но оставим на случай сложной иерархии)
        // if (other != _activationTrigger) { /* обычно не нужно сравнивать */ }

        TryFire(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // если урон только на pop — не реагируем на Stay
        if (_damageOnlyOnPop) return;
        if (!_armed || _busy) return;
        if (((1 << other.gameObject.layer) & _activatorLayers) == 0) return;

        TryFire(other);
    }

    private void TryFire(Collider activator)
    {
        if (Time.time - _lastFireTime < _cooldown) return;

        // запускаем корутину с анимацией и уроном
        StartCoroutine(FireRoutine(activator));
    }

    private IEnumerator FireRoutine(Collider activator)
    {
        _busy = true;
        _lastFireTime = Time.time;

        // FX старт
        if (_vfxDust != null) _vfxDust.Play();
        if (_audioSource && _sfxPop) _audioSource.PlayOneShot(_sfxPop);

        // Поднимаем шипы
        yield return MoveLocal(_spikesRoot, _downLocalPos, _upLocalPos, _riseTime);

        // Наносим урон
        DealDamageOnce(activator);

        // Держим наверху
        if (_holdTime > 0f) yield return new WaitForSeconds(_holdTime);

        // FX спуск
        if (_audioSource && _sfxRetract) _audioSource.PlayOneShot(_sfxRetract);

        // Опускаем
        yield return MoveLocal(_spikesRoot, _upLocalPos, _downLocalPos, _fallTime);

        _busy = false;

        if (_singleUse)
        {
            _armed = false;
            // можно просто выключить объект ловушки
            // gameObject.SetActive(false);
        }
    }

    private IEnumerator MoveLocal(Transform t, Vector3 from, Vector3 to, float time)
    {
        if (time <= 0f) { t.localPosition = to; yield break; }
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / time);
            float e = _curve != null ? _curve.Evaluate(k) : k;
            t.localPosition = Vector3.LerpUnclamped(from, to, e);
            yield return null;
        }
        t.localPosition = to;
    }

    private void DealDamageOnce(Collider activator)
    {
        if (activator == null) return;

        // Пытаемся найти получателя урона на активаторе или его родителях
        var dmg = activator.GetComponentInParent<PlayerStatManager>();
        if (dmg == null)
        {
            // Если у тебя свой компонент здоровья (например, PlayerHealth),
            // можно добавить адаптер:
            // var hp = activator.GetComponentInParent<PlayerHealth>();
            // if (hp) hp.ApplyDamage(_damage);
            return;
        }

        // точка контакта — позиция шипов (или ближайшая точка коллайдера)
        //Vector3 hitPoint = _spikesRoot != null ? _spikesRoot.position : transform.position;
        dmg.Damage(_damage);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_spikesRoot != null)
        {
            Gizmos.color = Color.red;
            var wpDown = _spikesRoot.parent ? _spikesRoot.parent.TransformPoint(_downLocalPos) : _downLocalPos;
            var wpUp   = _spikesRoot.parent ? _spikesRoot.parent.TransformPoint(_upLocalPos)   : _upLocalPos;
            Gizmos.DrawLine(wpDown, wpUp);
            Gizmos.DrawSphere(wpUp, 0.05f);
        }
    }
#endif
}
