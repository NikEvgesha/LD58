using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Нажимная плита: при наступании активатора вызывает Fire() у связанных ловушек.
/// Требуется Collider с isTrigger = true на этом же объекте (или дочернем).
/// </summary>
public class PressurePlate : ManagedBehaviour
{
    [Header("Activation")]
    [Tooltip("Какие слои могут нажимать плиту (например, Player)")]
    [SerializeField] private LayerMask _activatorLayers = ~0;
    [Tooltip("Можно ли повторно активировать (false = одноразовая)")]
    [SerializeField] private bool _repeatable = true;
    [Tooltip("Задержка между активациями (сек)")]
    [SerializeField, Min(0f)] private float _cooldown = 0.5f;

    [Header("Targets")]
    [Tooltip("Ловушки-стреломёты, которые выстрелят при нажатии")]
    [SerializeField] private WallArrowTrap[] _targets;

    [Header("FX (optional)")]
    [SerializeField] private Animator _anim;        // опционально, триггер "Press"
    [SerializeField] private string _pressTrigger = "Press";
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _sfxPress;

    private bool _usedOnce;
    private float _lastTriggerTime = -999f;
    public UnityEvent Use;

    private void Reset()
    {
        var col = GetComponentInChildren<Collider>(true);
        if (col) col.isTrigger = true;
        _anim = GetComponentInChildren<Animator>(true);
        _audio = GetComponentInChildren<AudioSource>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActivator(other)) return;

        if (!_repeatable && _usedOnce) return;
        if (Time.time - _lastTriggerTime < _cooldown) return;

        _lastTriggerTime = Time.time;
        _usedOnce = true;

        if (_anim && !string.IsNullOrEmpty(_pressTrigger)) _anim.SetTrigger(_pressTrigger);
        if (_audio && _sfxPress) _audio.PlayOneShot(_sfxPress);

        Use?.Invoke();
        /*
        if (_targets != null)
        {
            foreach (var t in _targets)
                if (t) t.Fire();
        }
        */
    }

    private bool IsActivator(Collider other)
    {
        return ((_activatorLayers.value & (1 << other.gameObject.layer)) != 0);
    }
}
