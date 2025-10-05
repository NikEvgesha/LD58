using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MarketItemData
{
    public string Name;
    public ItemType ItemType;
    public ConsumableType ConsumableType;
    public int Price;
    public Sprite Icon;
    public string Description;
}

[CreateAssetMenu(fileName ="MarketItemsList", menuName ="Scriptable/MarketItemsList")]
public class MarketItemsList : ScriptableObject
{
    [SerializeField] private List<MarketItemData> _data;

    public List<MarketItemData> Values => _data;
}