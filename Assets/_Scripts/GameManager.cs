using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<bool> GameEnd;
    public UnityEvent GameStart;



    private void Awake()
    {
        if (G.Game == null)
        {
            G.Game = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    public void OnGameStart()
    {
        GameStart?.Invoke();
        G.IsPaused = false;
        G.Control.CursorActive = false;
    }

    public void OnGameEnd(bool win = true)
    {
        G.IsPaused = true;
        // check if win
        GameEnd?.Invoke(win /*win*/);
        Transform spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;
        G.PlayerStatManager.transform.position = spawnPoint.position;
        G.SaveManager.SaveCollection(G.Inventory.Collected);
    }


}