using UnityEngine;

public class GameEndArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        G.Game.OnGameEnd(true);
    }
}
