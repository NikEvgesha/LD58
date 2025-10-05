using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : ManagedBehaviour
{
    [SerializeField] private GridLayoutGroup _mainGridContent;
    [SerializeField] private Transform _safeGridContent;
    [SerializeField] private InventorySlot _slotPrefab;
    [SerializeField] private GameObject _uiPanel;
    [SerializeField] private InputAction _inventoryAction;

    private List<InventorySlot> _mainSlots = new List<InventorySlot>();
    private List<InventorySlot> _safeSlots = new List<InventorySlot>();
    private bool _isOpen;
    private int _columns;


    private void Awake()
    {
        EnsureDefaultBindingsIfEmpty();
        RectTransform rect = _mainGridContent.GetComponent<RectTransform>();
        _columns = (int) (rect.rect.width / _mainGridContent.cellSize.x);
    }


    private void OnEnable()
    {
        if (_inventoryAction != null) _inventoryAction.Enable();
    }


    private void OnDisable()
    {
        G.Inventory.ItemsUpdated.RemoveListener(UpdateItems);
        G.Inventory.SafeItemsUpdated.RemoveListener(UpdateSafeItems);
        if (_inventoryAction != null) _inventoryAction.Disable();
    }

    private void Start()
    {
        G.Inventory.ItemsUpdated.AddListener(UpdateItems);
        G.Inventory.SafeItemsUpdated.AddListener(UpdateSafeItems);
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

}
