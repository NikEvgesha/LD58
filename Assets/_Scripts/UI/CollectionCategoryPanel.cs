using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectionCategoryPanel : MonoBehaviour
{
    [SerializeField] private CollectionSlot _slotPrefab;
    [SerializeField] private Transform _slotParent;
    [SerializeField] private TextMeshProUGUI _categoryTitle;

    public List<CollectionSlot> Init(CollectionCategoryData category)
    {
        List<CollectionSlot> slots = new();
        _categoryTitle.text = category.Name;
        foreach (CollectableItem item in category.Items)
        {
            CollectionSlot slot = Instantiate(_slotPrefab, _slotParent);
            slot.Init(item);
            slots.Add(slot);
        }

        return slots;
    }
}