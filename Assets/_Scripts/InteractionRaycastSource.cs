using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class InteractionRaycastSource : MonoBehaviour
{
    [SerializeField] private RaycastType _raycastType;
    [SerializeField] private float _raycastDistance;
    [SerializeField] private LayerMask _layer;

    private InteractionRaycastListener _lastHit;
    private Vector3 _direction;
    private Transform _camera;

    private void Awake()
    {


        Debug.Log(_direction);
    }
    private void FixedUpdate()
    {
        TryHit();
    }
    private bool TryHit()
    {
        switch (_raycastType)
        {

            case RaycastType.Down:
                _direction = Vector3.down;
                break;
            case RaycastType.Forward:
                _direction = transform.forward;
                break;
            default:
                _direction = Vector3.zero;
                break;
        }

        if (Physics.Raycast(transform.position, _direction, out RaycastHit hit, _raycastDistance, _layer) &&
            hit.transform.TryGetComponent(out InteractionRaycastListener listener))
        {
            if (_lastHit != listener)
            {
                if (_lastHit != null)
                    _lastHit.onRaycastFail();
                _lastHit = listener;
                _lastHit.onRaycastHit();

            }
            return true;
        }
        else
        {
            if (_lastHit != null)
            {
                _lastHit.onRaycastFail();
                _lastHit = null;
            }
            return false;
        }
    } 
}
