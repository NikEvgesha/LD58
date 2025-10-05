using UnityEngine;
using UnityEngine.UI;

public class CollectionSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;

    private bool _owned;
    private CollectableItem _item;

    public CollectableItem Item => _item;


    public void Init(CollectableItem item, bool owned=false)
    {
        _item = item;
        _icon.sprite = item.Data.Icon;
        SetOwned(owned);
    }

    public void SetOwned(bool owned)
    {
        _owned = owned;
        _icon.color = _owned ? Color.white : Color.black;
    }

}
