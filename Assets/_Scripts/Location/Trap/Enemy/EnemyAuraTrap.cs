using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Враг-ловушка с аурой: наносит урон каждую секунду всем игрокам в радиусе,
/// пока они внутри триггера. Не ходит, не атакует сам по себе.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class EnemyAuraTrap : DungeonTrap
{
    public enum DamageMode { HP, Fear, Both }

    [Header("Activation")]
    [Tooltip("Какие слои считаем игроками/целями (обычно слой Player)")]
    [SerializeField] private LayerMask _targetLayers = ~0;
    [Tooltip("Можно ли работать сразу после спавна (иначе ждём Activate())")]
    [SerializeField] private bool _autoArmOnStart = true;

    [Header("Area")]
    [Tooltip("Радиус ауры урона (берётся из SphereCollider.radius, но можно менять тут)")]
    [SerializeField, Min(0.1f)] private float _radius = 3f;
    [Tooltip("Нужна ли прямая видимость (Raycast) до цели")]
    [SerializeField] private bool _requireLineOfSight = false;
    [Tooltip("Слои, считающиеся преградой для обзора")]
    [SerializeField] private LayerMask _obstacleLayers = (1 << 0); // Default

    [Header("Damage")]
    [SerializeField, Min(1)] private int _damagePerTick = 5;
    [SerializeField, Min(0.1f)] private float _tickInterval = 1.0f;
    [SerializeField] private DamageMode _damageMode = DamageMode.HP;

    [Header("FX (optional)")]
    [SerializeField] private ParticleSystem _enterFX;
    [SerializeField] private ParticleSystem _tickFX;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _sfxEnter;
    [SerializeField] private AudioClip _sfxTick;

    // --- runtime ---
    private readonly HashSet<PlayerStatManager> _victims = new HashSet<PlayerStatManager>();
    private SphereCollider _trigger;
    private bool _armed;
    private Coroutine _tickLoop;

    private void Reset()
    {
        _trigger = GetComponent<SphereCollider>();
        _trigger.isTrigger = true;
        _trigger.radius = _radius;
    }

    private void Awake()
    {
        _trigger = GetComponent<SphereCollider>();
        _trigger.isTrigger = true;
        _trigger.radius = _radius;
    }

    private void Start()
    {
        if (_autoArmOnStart) Activate();
    }

    public override void Activate()
    {
        _armed = true;
        // держим радиус в коллайдере синхронным с полем
        if (_trigger) _trigger.radius = _radius;

        if (_tickLoop == null)
            _tickLoop = StartCoroutine(TickRoutine());
    }
/*
    public override void Deactivate()
    {
        _armed = false;
        if (_tickLoop != null)
        {
            StopCoroutine(_tickLoop);
            _tickLoop = null;
        }
        _victims.Clear();
    }
*/

    private void OnValidate()
    {
        if (_radius < 0.1f) _radius = 0.1f;
        if (_tickInterval < 0.1f) _tickInterval = 0.1f;
        // поддерживаем Radius в инспекторе
        var sc = GetComponent<SphereCollider>();
        if (sc && sc.isTrigger) sc.radius = _radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_armed) return;
        if (!IsTarget(other)) return;

        var psm = other.GetComponentInParent<PlayerStatManager>();
        if (psm != null && _victims.Add(psm))
        {
            if (_enterFX) _enterFX.Play();
            if (_audio && _sfxEnter) _audio.PlayOneShot(_sfxEnter);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var psm = other.GetComponentInParent<PlayerStatManager>();
        if (psm != null) _victims.Remove(psm);
    }

    private bool IsTarget(Collider col)
    {
        return ((_targetLayers.value & (1 << col.gameObject.layer)) != 0);
    }

    private IEnumerator TickRoutine()
    {
        var wait = new WaitForSeconds(_tickInterval);

        while (true)
        {
            if (_armed && _victims.Count > 0)
            {
                // чтобы не модифицировать set в ходе обхода
                var snapshot = ListCache<PlayerStatManager>.Get();
                snapshot.AddRange(_victims);

                foreach (var psm in snapshot)
                {
                    if (psm == null) { _victims.Remove(psm); continue; }

                    // проверка видимости по желанию
                    if (_requireLineOfSight && !HasLineOfSight(psm))
                        continue;

                    ApplyDamage(psm);
                }

                ListCache<PlayerStatManager>.Release(snapshot);

                if (_tickFX) _tickFX.Play();
                if (_audio && _sfxTick) _audio.PlayOneShot(_sfxTick);
            }

            yield return wait;
        }
    }

    private bool HasLineOfSight(PlayerStatManager psm)
    {
        // пускаем луч от центра ауры до центра капсулы/трансформа игрока
        var origin = transform.position;
        var target = psm.transform.position + Vector3.up * 0.9f;

        if (Physics.Linecast(origin, target, out var hit, _obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            // если упёрлись в препятствие раньше, чем в цель — нет видимости
            return hit.collider.GetComponentInParent<PlayerStatManager>() == psm;
        }
        return true; // ничто не мешает
    }

    private void ApplyDamage(PlayerStatManager psm)
    {
        switch (_damageMode)
        {
            case DamageMode.HP:
                psm.Damage(_damagePerTick, PlayerStats.HP);
                break;
            case DamageMode.Fear:
                psm.Damage(_damagePerTick, PlayerStats.Fear);
                break;
            case DamageMode.Both:
                psm.Damage(_damagePerTick, PlayerStats.HP);
                psm.Damage(_damagePerTick, PlayerStats.Fear);
                break;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
        Gizmos.DrawSphere(transform.position, _radius);
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
#endif

    /// <summary>
    /// маленький кеш для списков, чтобы не аллоцировать каждый тик
    /// </summary>
    static class ListCache<T>
    {
        static readonly Stack<List<T>> _pool = new Stack<List<T>>();
        public static List<T> Get() => _pool.Count > 0 ? _pool.Pop() : new List<T>(8);
        public static void Release(List<T> list) { list.Clear(); _pool.Push(list); }
    }
}
