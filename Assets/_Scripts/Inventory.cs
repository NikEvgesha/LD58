using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int _safeBagCapacity = 3;
    [SerializeField] private MarketItemsList _marketItems;
    private List<CollectableItem> _items;
    private List<CollectableItem> _safeItems;
    private HashSet<CollectableItemData> _collected;
    private Dictionary<MarketItemData, int> _consumables;
    public bool IsReady { get; private set; }
    public int SafeBagCapacity => _safeBagCapacity;
    public HashSet<CollectableItemData> Collected => _collected;

    [HideInInspector]
    public UnityEvent<ReadOnlyCollection<CollectableItem>> ItemsUpdated;
    [HideInInspector]
    public UnityEvent<ReadOnlyCollection<CollectableItem>> SafeItemsUpdated;
    [HideInInspector]
    public UnityEvent NoSpaceInSafeBag;


    public ReadOnlyCollection<CollectableItem> Items => _items.AsReadOnly();
    public ReadOnlyCollection<CollectableItem> SafeItems => _safeItems.AsReadOnly();
    public ReadOnlyCollection<CollectableItem> AllItems => _items.Concat(_safeItems).ToList().AsReadOnly();

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
        _collected = new();
        _consumables = new();
    }

    private void Start()
    {
        foreach (MarketItemData item in _marketItems.Values)
        {
            _consumables.Add(item, 0);
        }
             
        IsReady = true;
    }






    public void Add(CollectableItem item, bool tryAddSafe=true)
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
        
        if (!_collected.Contains(item.Data))
        {
            _collected.Add(item.Data);
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


    public List<KeyValuePair<CollectableItemData, int>> GetResultItems(bool all)
    {
        
        Dictionary<CollectableItemData, int> itemsCount = new Dictionary<CollectableItemData, int>();

        foreach (CollectableItem item in _safeItems)
        {
            if (itemsCount.ContainsKey(item.Data))
            {
                itemsCount[item.Data]++;
            }
            else
            {
                itemsCount.Add(item.Data, 1);
            }
        }


        if (all)
        {
            foreach (CollectableItem item in _items)
            {
                if (itemsCount.ContainsKey(item.Data))
                {
                    itemsCount[item.Data]++;
                }
                else
                {
                    itemsCount.Add(item.Data, 1);
                }
            }
        }


        return itemsCount.OrderByDescending(x => x.Value).ToList();
    }


    public void TryMoveToSafe(CollectableItem item)
    {
        if (_safeItems.Count >= _safeBagCapacity)
        {
            NoSpaceInSafeBag?.Invoke();
            return;
        }

        _items.Remove(item);
        _safeItems.Add(item);
        SafeItemsUpdated?.Invoke(_safeItems.AsReadOnly());
        ItemsUpdated?.Invoke(_items.AsReadOnly());

    }

    public void MoveToMain(CollectableItem item)
    {
        _safeItems.Remove(item);
        _items.Add(item);
        SafeItemsUpdated?.Invoke(_safeItems.AsReadOnly());
        ItemsUpdated?.Invoke(_items.AsReadOnly());
    }



    public void AddConsumable(MarketItemData itemData)
    {
        if (_consumables.ContainsKey(itemData)) {
            _consumables[itemData]++;
        }
    }


    public bool RemoveConsumable(MarketItemData itemData)
    {
        if (_consumables.ContainsKey(itemData) && _consumables[itemData] > 0) {
            _consumables[itemData]--;
            return true;
        }

        return false;
    }

}
