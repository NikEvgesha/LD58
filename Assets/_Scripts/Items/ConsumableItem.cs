using UnityEngine;



public class ConsumableItem : MarketItemsList
{
    [SerializeField] private ConsumableType _consumableType;
    public ConsumableType ConsumableType => _consumableType;

}