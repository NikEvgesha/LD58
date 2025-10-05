using System;
using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private Sprite _coinIcon;
    [SerializeField] private int _startCoinsAmount;

    [SerializeField] private AudioClip _audioBuy;
    [SerializeField] private AudioClip _audioSell;
    [SerializeField] private AudioSource _audioSource;
    private int _amount = 0;
    public int Coins { get { return _amount; } }

    public UnityEvent<int> CurrencyChanged;
    public UnityEvent NoCoins;

    private void Awake()
    {
        if (G.Currency == null)
        {
            G.Currency = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        
        AddCurrency(_startCoinsAmount);
    }


    public void Reset()
    {
        _amount = 0;
        AddCurrency(_startCoinsAmount);
    }

    public void AddCurrency( int amount)
    {
        //if (_audioSource)
        //    if(_audioSell)
        //        _audioSource.PlayOneShot(_audioSell);
        _amount += amount;
        CurrencyChanged?.Invoke(_amount);
       //G.SaveManager.SaveGameCoin(_balance[type]);
    }

    public bool RemoveCurrency(int amount)
    {
        if (_amount >= amount)
        {
            //if (_audioSource)
            //    if (_audioBuy)
            //        _audioSource.PlayOneShot(_audioBuy);

            _amount -= amount;
            CurrencyChanged?.Invoke(_amount);
            //G.SaveManager.SaveGameCoin(_balance[type]);
            return true;
        }
        NoCoins?.Invoke();
        return false;
    }




    public Sprite GetCurrencyIcon()
    {
        return _coinIcon;
    }
    
}
