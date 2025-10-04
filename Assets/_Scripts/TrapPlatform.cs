using UnityEngine;

public class TrapPlatform : MonoBehaviour
{
    public void _OnPlayerEnter()
    {
        Debug.Log("On Platform");
    }

    public void _OnPlayerExit()
    {
        Debug.Log("Exit Platform");
    }
}
