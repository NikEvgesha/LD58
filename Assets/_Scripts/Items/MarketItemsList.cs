using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="MarketItemsList", menuName ="Scriptable/MarketItemsList")]
public class MarketItemsList : ScriptableObject
{
    [SerializeField] private List<MarketItemData> _data;

    public List<MarketItemData> Values => _data;
}