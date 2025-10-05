using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] private Transform _target; // игрок (можно указать вручную или найти)
    [SerializeField] private bool _findPlayerAutomatically = true;
    [SerializeField] private float _rotationSpeed = 5f;

    private void Start()
    {
        if (_findPlayerAutomatically && _target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) _target = player.transform;
        }
    }

    private void Update()
    {
        if (!_target) return;

        // направление к игроку, но по плоской плоскости XZ
        Vector3 direction = _target.position - transform.position;
        direction.y = 0f; // убираем наклон вверх/вниз

        if (direction.sqrMagnitude < 0.001f)
            return; // слишком близко — не вращаем

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * _rotationSpeed);
    }
}
