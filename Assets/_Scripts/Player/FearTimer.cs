using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FearTimer : ManagedBehaviour
{
    [SerializeField] private int _damage;
    void Start()
    {
        StartTimer();
    }
    public void StartTimer()
    {
        StartCoroutine(StartTimerCoroutine());
    }
    private IEnumerator StartTimerCoroutine()
    {
        while (true)
        {
            if (!G.IsPaused)
            {
                G.PlayerStatManager.Damage(_damage, PlayerStats.Fear);
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                yield return null;
            }
        }

    }
    void OnDestroy()
    {
        StopAllCoroutines();
    }
}
