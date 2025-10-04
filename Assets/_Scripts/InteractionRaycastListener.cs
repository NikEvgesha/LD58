using UnityEngine;
using UnityEngine.Events;

public class InteractionRaycastListener : MonoBehaviour
{
    [SerializeField] public UnityEvent _hitEvent;
    [SerializeField] public UnityEvent _noHitEvent;

    private void OnDestroy()
    {
        _hitEvent.RemoveAllListeners();
        _noHitEvent.RemoveAllListeners();
    }

    public void onRaycastHit()
    {
        Debug.Log("item hit");
        _hitEvent?.Invoke();
    }

    public void onRaycastFail()
    {
        _noHitEvent?.Invoke();
    }
}
