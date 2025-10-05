using UnityEngine;

public class MarketUI : MonoBehaviour 
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private MarketItemsList _marketItems;
    [SerializeField] private MarketSlot _slotPrefab;
    [SerializeField] private Transform _slotParent;



    private void Start()
    {
        foreach (var item in _marketItems.Values) 
        {
            MarketSlot slot = Instantiate(_slotPrefab, _slotParent);
            slot.Init(item);
        }
    }


    public void _Open()
    {
        _panel.SetActive(true);
    }

    public void _Close()
    {
        _panel.SetActive(false);
        G.Game.OnGameStart();
    }

}