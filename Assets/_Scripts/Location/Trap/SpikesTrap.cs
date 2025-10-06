using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ловушка "шипы": поднимаются при наступании игрока, наносят урон
/// ТОЛЬКО если к концу анимации игрок всё ещё в зоне.
/// </summary>
public class SpikesTrap : DungeonTrap
{
    [Header("Refs")]
    [SerializeField] private Collider _activationTrigger;
    [SerializeField] private Transform _spikesRoot;

    [Header("Animation")]
    [SerializeField] private Vector3 _downLocalPos = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 _upLocalPos = new Vector3(0, 1.2f, 0);
    [SerializeField, Min(0.01f)] private float _riseTime = 0.15f;
    [SerializeField, Min(0f)] private float _holdTime = 0.25f;
    [SerializeField, Min(0.01f)] private float _fallTime = 0.2f;
    [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _curveDone = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Gameplay")]
    [SerializeField] private LayerMask _activatorLayers = ~0;
    [SerializeField, Min(0)] private int _damage = 10;
    [SerializeField, Min(0f)] private float _cooldown = 1.25f;
    [SerializeField] private bool _singleUse = false;

    [Tooltip("Требовать присутствие жертвы в зоне к моменту удара (конец анимации)")]
    [SerializeField] private bool _requirePresenceAtImpact = true;

    [Header("FX (optnl)")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _sfxPop;
    [SerializeField] private AudioClip _sfxRetract;
    [SerializeField] private ParticleSystem _vfxDust;

    private bool _armed = false;
    private bool _busy = false;
    private float _lastFireTime = -999f;
    private bool _hasDamage;

    // кто сейчас стоит на триггере
    private readonly HashSet<Collider> _inside = new HashSet<Collider>();

    private void Reset()
    {
        if (_activationTrigger == null)
        {
            _activationTrigger = GetComponentInChildren<Collider>(true);
            if (_activationTrigger) _activationTrigger.isTrigger = true;
        }
        if (_spikesRoot == null) _spikesRoot = transform;
    }

    private void Awake()
    {
        if (_spikesRoot == null) _spikesRoot = transform;
        _spikesRoot.localPosition = _downLocalPos;

        if (_activationTrigger == null)
        {
            _activationTrigger = GetComponentInChildren<Collider>(true);
            if (_activationTrigger) _activationTrigger.isTrigger = true;
        }
    }

    public override void Activate()
    {
        _armed = true;
        _busy = false;
        _spikesRoot.localPosition = _downLocalPos;
        _inside.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _activatorLayers) == 0) return;
        _inside.Add(other);

        if (!_armed || _busy || _activationTrigger == null) return;
        // момент активации стартует при первом входе
        TryFire(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _inside.Remove(other);
    }

    private void TryFire(Collider activator)
    {
        if (Time.time - _lastFireTime < _cooldown) return;
        StartCoroutine(FireRoutine(activator));
    }

    private IEnumerator FireRoutine(Collider activator)
    {
        _busy = true;
        _lastFireTime = Time.time;

        if (_vfxDust) _vfxDust.Play();

        // подъём
        yield return MoveLocal(_spikesRoot, _downLocalPos, _upLocalPos, _riseTime, _curve);

        if (_audioSource && _sfxPop) _audioSource.PlayOneShot(_sfxPop);
        // УДАР: только если цель всё ещё в зоне (или проверку можно выключить)
        bool shouldHit = true;
        if (_requirePresenceAtImpact)
        {
            // если конкретный активатор ещё внутри — бьём его;
            // иначе — не бьём никого
            shouldHit = activator != null && _inside.Contains(activator);
        }

        if (shouldHit)
        {
            DealDamageOnce(activator);
        }
        // удержание наверху
        if (_holdTime > 0f) yield return new WaitForSeconds(_holdTime);


        if (_audioSource && _sfxRetract) _audioSource.PlayOneShot(_sfxRetract);

        // спуск
        yield return MoveLocal(_spikesRoot, _upLocalPos, _downLocalPos, _fallTime,_curveDone);

        _busy = false;

        if (_singleUse)
        {
            _armed = false;
            // gameObject.SetActive(false);
        }
    }

    private IEnumerator MoveLocal(Transform t, Vector3 from, Vector3 to, float time, AnimationCurve animC)
    {
        if (time <= 0f) { t.localPosition = to; yield break; }
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / time);
            float e = animC != null ? animC.Evaluate(k) : k;
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

        Vector3 hitPoint = _spikesRoot ? _spikesRoot.position : transform.position;
        dmg.Damage(_damage);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_spikesRoot != null)
        {
            Gizmos.color = Color.red;
            var p0 = _spikesRoot.parent ? _spikesRoot.parent.TransformPoint(_downLocalPos) : _downLocalPos;
            var p1 = _spikesRoot.parent ? _spikesRoot.parent.TransformPoint(_upLocalPos) : _upLocalPos;
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawSphere(p1, 0.05f);
        }
    }
#endif
}
