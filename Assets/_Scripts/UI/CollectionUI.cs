using System.Collections.Generic;
using UnityEngine;

public class CollectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private CollectionCategoryPanel _categoryPanelPrefab;
    [SerializeField] private Transform _categoryParent;
    [SerializeField] private List<CollectionCategoryData> _categories;

    private List<CollectionSlot> _slots = new();
    private bool _isOpened;
    private List<string> _loadedCollected = new List<string>();
    private List<CollectableItemData> _colected = new List<CollectableItemData>();


    private void Start()
    {
        foreach (var category in _categories) 
        {
            CollectionCategoryPanel categoryPanel = Instantiate(_categoryPanelPrefab, _categoryParent);
            _slots.AddRange(categoryPanel.Init(category));
        }
        _loadedCollected = G.SaveManager.LoadCollection();

        foreach (var slot in _slots)
        {
            bool collected = _loadedCollected.Contains(slot.Item.Data.Name);
            slot.SetOwned(collected);
            if (collected)
                _colected.Add(slot.Item.Data);
        }

        G.Inventory.SetCollected(_colected);

    }


    public void ToggleOpen()
    {
        _isOpened = !_isOpened;
        CheckCollected();
        _panel.SetActive(_isOpened);
    }


    private void CheckCollected()
    {
        foreach (var slot in _slots)
        {
            slot.SetOwned(G.Inventory.Collected.Contains(slot.Item.Data));
        }
    }




}
