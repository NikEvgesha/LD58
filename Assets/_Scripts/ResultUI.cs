using TMPro;
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    [SerializeField] GameObject _resultPanel;
    [SerializeField] Transform _itemSlotsParent;
    [SerializeField] TextMeshProUGUI _totalValueText;
    [SerializeField] GameResultItemSlot _slotPrefab;

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
        _resultPanel.SetActive(true);
        G.Control.CursorActive = true;
        _totalValue = 0;
        foreach (var item in G.Inventory.GetResultItems(win)) {
            GameResultItemSlot slot = Instantiate(_slotPrefab, _itemSlotsParent);
            slot.Init(item.Key, item.Value);
            _totalValue += item.Key.Price * item.Value;
        }

        _totalValueText.text = _totalValue.ToString();

        G.Currency.AddCurrency(_totalValue);

    }

}
