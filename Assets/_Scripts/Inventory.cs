using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int _safeBagCapacity = 3;
    private List<CollectableItem> _items;
    private List<CollectableItem> _safeItems;
    public bool IsReady { get; private set; }
    public int SafeBagCapacity => _safeBagCapacity;

    [HideInInspector]
    public UnityEvent<ReadOnlyCollection<CollectableItem>> ItemsUpdated;
    [HideInInspector]
    public UnityEvent<ReadOnlyCollection<CollectableItem>> SafeItemsUpdated;


    public ReadOnlyCollection<CollectableItem> Items => _items.AsReadOnly();

    private void Awake()
    {
        if (G.Inventory == null)
        {
            G.Inventory = this;
        }
        else
        {
            Destroy(this);
        }
        _items = new();
        _safeItems = new();
    }

    private void Start()
    {
        IsReady = true;
    }



    public void Add(CollectableItem item)
    {
        
        if (_safeItems.Count < _safeBagCapacity)
        {
            _safeItems.Add(item);
            SafeItemsUpdated?.Invoke(_safeItems.AsReadOnly());
        } else
        {
            _items.Add(item);
            ItemsUpdated?.Invoke(_items.AsReadOnly());
        }
            
        //item.transform.SetParent(PlayerManager.Instance.transform);
        item.gameObject.SetActive(false);
        
    }

    public void Remove(CollectableItem item)
    {
        if (_safeItems.Remove(item))
        {
            SafeItemsUpdated?.Invoke(_safeItems.AsReadOnly());
        } else if (_items.Remove(item))
        {
            ItemsUpdated?.Invoke(_items.AsReadOnly());
        }
            
        //item.position = 
        
    }
}
