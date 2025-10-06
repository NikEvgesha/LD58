using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _amount;

    private Animator _animator;

    private MarketItemData _item;

    public void SetItem(MarketItemData item, int amount=0)
    {
        _animator = GetComponent<Animator>();
        _item = item;
        _icon.sprite = item.Icon;
        _amount.text = "x" + amount.ToString();
        G.Inventory.ConsumablesUpdated.AddListener(CheckUpdate);
        G.Inventory.NoConsumable.AddListener(NoItem);
    }

    private void CheckUpdate(MarketItemData item, int amount)
    {
        if (_item == null) return;
        if (_item.Name == item.Name)
        {
            _amount.text = "x" + amount.ToString();
        }
    }


    private void NoItem(MarketItemData item)
    {
        if (_item != null && _item == item)
        {
            _animator.SetTrigger("NoItem");
        }
        
    }

}
