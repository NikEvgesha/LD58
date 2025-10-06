using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _price;

    private Button _button;

    private MarketItemData _itemData;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }
    private void OnEnable()
    {
        G.Currency.CurrencyChanged.AddListener(CheckPrice);
        CheckPrice(G.Currency.Coins);
    }

    public void Init(MarketItemData data)
    {
        _itemData = data;
        _icon.sprite = data.Icon;
        _name.text = data.Name;
        _description.text = data.Description;
        _price.text = data.Price.ToString();
    }

    public void _OnClick()
    {
        if (G.Currency.RemoveCurrency(_itemData.Price))
        {
            G.Inventory.AddConsumable(_itemData);
        }
    }

    private void CheckPrice(int amount)
    {
        if (G.Currency.Coins < _itemData.Price)
        {
            _price.color = Color.red;
            _button.interactable = false;
        }
        else
        {
            _price.color = Color.white;
            _button.interactable = true;
        }
    }
}