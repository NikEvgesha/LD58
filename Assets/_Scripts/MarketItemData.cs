using UnityEngine;

[CreateAssetMenu(fileName = "MarketItem", menuName = "Scriptable/MarketItem")]
public class MarketItemData : ScriptableObject {

    [SerializeField] private string _name;
    [SerializeField] private ItemType _itemType;
    [SerializeField] private ConsumableType _consumableType;
    [SerializeField] private int _price;
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _description;

    public string Name => _name;
    public ItemType ItemType => _itemType;
    public ConsumableType ConsumableType => _consumableType;
    public int Price => _price;
    public Sprite Icon => _icon;
    public string Description => _description;

}
