using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : ManagedBehaviour
{
    [SerializeField] private GridLayoutGroup _mainGridContent;
    [SerializeField] private Transform _safeGridContent;
    [SerializeField] private Transform _consumablesParent;
    [SerializeField] private Transform _usableParent;
    [SerializeField] private InventorySlot _slotPrefab;
    [SerializeField] private ConsumableSlot _consumableSlotPrefab;
    [SerializeField] private UsableSlot _usableSlotPrefab;
    [SerializeField] private GameObject _uiPanel;
    [SerializeField] private InputAction _inventoryAction;
    [SerializeField] private MarketItemsList _marketList;


    private List<InventorySlot> _mainSlots = new List<InventorySlot>();
    private List<InventorySlot> _safeSlots = new List<InventorySlot>();
    private List<ConsumableSlot> _consumableSlots = new List<ConsumableSlot>();
    private List<UsableSlot> _usableSlots = new List<UsableSlot>();
    private bool _isOpen;
    private int _columns;


    [SerializeField] private InputAction _usable1;
    [SerializeField] private InputAction _usable2;
    [SerializeField] private InputAction _usable3;


    private void Awake()
    {
        EnsureDefaultBindingsIfEmpty();
        RectTransform rect = _mainGridContent.GetComponent<RectTransform>();
        _columns = (int) (rect.rect.width / _mainGridContent.cellSize.x);
    }


    private void OnEnable()
    {
        if (_inventoryAction != null) _inventoryAction.Enable();
        if (_usable1 != null) _usable1.Enable();
        if (_usable2 != null) _usable2.Enable();
        if (_usable3 != null) _usable3.Enable();
    }


    private void OnDisable()
    {
        G.Inventory.ItemsUpdated.RemoveListener(UpdateItems);
        G.Inventory.SafeItemsUpdated.RemoveListener(UpdateSafeItems);
        if (_inventoryAction != null) _inventoryAction.Disable();
        if (_usable1 != null) _usable1.Disable();
        if (_usable2 != null) _usable2.Disable();
        if (_usable3 != null) _usable3.Disable();
    }

    private void Start()
    {
        G.Inventory.ItemsUpdated.AddListener(UpdateItems);
        G.Inventory.SafeItemsUpdated.AddListener(UpdateSafeItems);
        G.Inventory.ConsumablesUpdated.AddListener(UpdateConsumables);
        for (int i = 0; i < _columns; i++)
        {
            InventorySlot slot = Instantiate(_slotPrefab, _mainGridContent.transform);
            slot.Init(null, false);
            _mainSlots.Add(slot);
        }

        for (int i = 0; i < G.Inventory.SafeBagCapacity; i++)
        {
            InventorySlot slot = Instantiate(_slotPrefab, _safeGridContent);
            slot.Init(null, true);
            _safeSlots.Add(slot);
        }

        foreach (var item in _marketList.Values)
        {
            switch (item.ItemType) {
                case ItemType.Consumable:
                    ConsumableSlot slot = Instantiate(_consumableSlotPrefab, _consumablesParent);
                    slot.SetItem(item);
                    _consumableSlots.Add(slot);
                    break;
                case ItemType.Usable:
                    UsableSlot usableSlot = Instantiate(_usableSlotPrefab, _usableParent);
                    _usableSlots.Add(usableSlot);
                    usableSlot.SetIndex(_usableSlots.Count);
                    break;
                default: break;
            }
        }


    }

    private void EnsureDefaultBindingsIfEmpty()
    {
        if (_inventoryAction == null || _inventoryAction.bindings.Count == 0)
        {
            _inventoryAction = new InputAction("Inventory", InputActionType.Button);
            _inventoryAction.AddBinding("<Keyboard>/B");
        }
    }

    protected override void PausableUpdate()
    {
        if (_inventoryAction.triggered)
        {
            _ToggleOpen();
        }

        if (_usable1.triggered)
        {
            UsableClicked(1);
        }
        if (_usable2.triggered)
        {
            UsableClicked(2);
        }
        if (_usable3.triggered)
        {
            UsableClicked(3);
        }

    }

    public void _ToggleOpen()
    {
        _isOpen = !_isOpen;
        _uiPanel.SetActive(_isOpen);
        G.Control.CursorActive = _isOpen;
    }


    private void UpdateItems(ReadOnlyCollection<CollectableItem> items)
    {
        int i = 0;
        for (; i < _mainSlots.Count; i++)
        {
            if (i >= items.Count)
            {

                if (i >= items.Count + _columns - (items.Count % _columns))
                {
                    Destroy(_mainSlots[i].gameObject);
                    _mainSlots.RemoveAt(i);
                    i--;
                } else
                {
                    _mainSlots[i].Init(null, false);
                }
                continue;
            }

            if (_mainSlots[i].Item != items[i])
            {
                _mainSlots[i].Init(items[i], false);
            }
        }

        for (;i < items.Count; i++)
        {
            InventorySlot slot = Instantiate(_slotPrefab, _mainGridContent.transform);
            slot.Init(items[i], false);
            _mainSlots.Add(slot);
        }

    }


    private void UpdateSafeItems(ReadOnlyCollection<CollectableItem> items)
    {
        int i = 0;
        for (; i < _safeSlots.Count; i++)
        {
            if (i >= items.Count)
            {
                _safeSlots[i].Init(null, true);
                
            } else if (_safeSlots[i].Item != items[i])
            {
                _safeSlots[i].Init(items[i], true);
            }

            
        }

    }


    public void UsableClicked(int index)
    {
        if (!_usableSlots[index-1].Empty)
        {
            G.Inventory.TryUse(_usableSlots[index - 1].Item);
        }
    }


    public void UpdateConsumables(MarketItemData item, int amount)
    {
        if (item.ItemType == ItemType.Usable) {
            UsableSlot slot = _usableSlots.Find(x => x.Item == item);
            if (slot == null)
            {
                slot = _usableSlots.Find(x => x.Empty);
                if (slot != null)
                {
                    slot.SetItem(item, amount);
                }
            }
        }
    } 

}
