using System;
using UnityEngine;

/// <summary>
/// Стрела: летит по направлению, наносит урон при столкновении, затем самоудаляется.
/// Требует Rigidbody (useGravity на твой вкус) и Collider (не-trigger).
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ArrowProjectile : ManagedBehaviour
{
    [Header("Damage")]
    [SerializeField, Min(0)] private int _damage = 10;
    [SerializeField] private LayerMask _hitLayers = ~0; // какие слои получать урон

    [Header("Lifetime")]
    [SerializeField, Min(0f)] private float _lifeTime = 6f;
    [SerializeField] private bool _destroyOnHit = true;
    [Tooltip("Залипать в объект при попадании (выключает Rigidbody)")]
    [SerializeField] private bool _stickOnHit = true;

    private Rigidbody _rb;
    private bool _hasHit;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float speed)
    {
        if (_rb)
        {
            _rb.linearVelocity = direction.normalized * speed;
            // Для “баллистики” включи _rb.useGravity = true и подстрой скорость.
        }
        if (_lifeTime > 0f) Destroy(gameObject, _lifeTime);
    }

    private void LateUpdate()
    {
        // Поворачиваем стрелу по направлению скорости (красиво летит остриём вперёд)
        if (_rb && _rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.forward = _rb.linearVelocity.normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;

        // фильтр слоёв
        if (((1 << collision.gameObject.layer) & _hitLayers) == 0)
        {
            if (_destroyOnHit) Destroy(gameObject);
            return;
        }

        // урон
        var dmg = collision.gameObject.GetComponentInParent<PlayerStatManager>();
        if (dmg != null)
        {
            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            dmg.Damage(_damage);
        }

        _hasHit = true;

        if (_stickOnHit)
        {
            if (_rb)
            {
                _rb.isKinematic = true;
                _rb.detectCollisions = false;
            }
            transform.SetParent(collision.collider.transform, true);
        }

        if (_destroyOnHit && !_stickOnHit)
            Destroy(gameObject);
    }
}
