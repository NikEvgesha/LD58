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
        G.IsPaused = false;
        GameStart?.Invoke();
    }

    public void OnGameEnd()
    {
        G.IsPaused = true;
        // check if win
        GameEnd?.Invoke(true /*win*/);
    }


}