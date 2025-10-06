using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UsableSlot : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _indexText;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _amount;

    private MarketItemData _item = null;
    private int _index;
    private bool _empty = true;
    private Button _button;

    public bool Empty => _empty;
    public MarketItemData Item => _item;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void Start()
    {
        if (_empty)
        {
            SetActive(false);    
        }
    }

    public void SetItem(MarketItemData item, int amount)
    {
        _item = item;
        _icon.sprite = item.Icon;
        _amount.text = amount.ToString();
        SetActive(true);
        G.Inventory.ConsumablesUpdated.AddListener(CheckUpdate);
    }

    private void CheckUpdate(MarketItemData item, int amount)
    {
        if (_item == null) return;
        if (_item.Name == item.Name)
        {
            if (amount > 0)
            {
                _amount.text = amount.ToString();
            } else
            {
                _item = null;
                SetActive(false);
            }
        }
    }

    public void SetIndex(int index) 
    {
        _index = index;
        _indexText.text = _index.ToString();
    }

    private void SetActive(bool active)
    {
        _icon.gameObject.SetActive(active);
        _amount.gameObject.SetActive(active);
        _empty = !active;
        _button.interactable = active;
    }

    public void _OnClick()
    {
        G.Inventory.TryUse(_item);
    }


}
