using UnityEngine;

public class ManagedBehaviour : MonoBehaviour
{
    private void Update()
    {
        if (!G.IsPaused)
        {
            PausableUpdate();
        }
    }

    protected virtual void PausableUpdate()
    {

    }


    private void FixedUpdate()
    {
        if (!G.IsPaused)
        {
            PausableFixedUpdate();
        }
    }

    protected virtual void PausableFixedUpdate()
    {

    }
}