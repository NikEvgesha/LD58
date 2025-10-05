using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(fileName ="CollectionCategory", menuName = "Scriptable/CollectionCategory")]
public class CollectionCategoryData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private List<CollectableItem> _items;


    public string Name => _name;
    public ReadOnlyCollection<CollectableItem> Items => _items.AsReadOnly();
}