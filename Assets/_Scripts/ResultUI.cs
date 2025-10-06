using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    [SerializeField] GameObject _resultPanel;
    [SerializeField] Transform _itemSlotsParent;
    [SerializeField] TextMeshProUGUI _totalValueText;
    [SerializeField] GameResultItemSlot _slotPrefab;

    private List<GameResultItemSlot> _slots = new();

    private int _totalValue;
    // TODO: AddListener to GameEnd Event

    private void OnEnable()
    {
        G.Game.GameEnd.AddListener(OnGameEnd);
    }

    private void OnDisable()
    {
        G.Game.GameEnd.RemoveListener(OnGameEnd);
    }


    public void OnGameEnd(bool win)
    {
        foreach (var slot in _slots)
        {
            Destroy(slot.gameObject);
        }

        _slots.Clear();

        //while (_slots.Count > 0)
        //{
        //    DestroyImmediate(_slots.Last());
        //}

       
        _totalValue = 0;
        foreach (var item in G.Inventory.GetResultItems(win)) {
            GameResultItemSlot slot = Instantiate(_slotPrefab, _itemSlotsParent);
            slot.Init(item.Key, item.Value);
            _totalValue += item.Key.Price * item.Value;
            _slots.Add(slot);
        }

        _totalValueText.text = _totalValue.ToString();
        _resultPanel.SetActive(true);
        G.Control.CursorActive = true;
        G.Currency.AddCurrency(_totalValue);

    }

    public void _Close()
    {
        _resultPanel.SetActive(false);
    }

}
