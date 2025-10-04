using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Text _name;
    [SerializeField] private GameObject _quickSlotIndicator;
    [SerializeField] private Image _background;

    private CollectableItem _item;
    public CollectableItem Item => _item;

    public void Init(CollectableItem item, bool showBackGround = false)
    {
        _item = item;

        _background.gameObject.SetActive(showBackGround || item != null);
        _icon.gameObject.SetActive(item != null);
        _name.gameObject.SetActive(item != null);

        if (item == null)
        {
            return;
        }

        _icon.sprite = item.Data.Icon;
        _name.text = item.Data.Name;

        //switch (item.Data.Rarity)
        //{
        //    case Rarity.Common:
        //        _background.color = Color.gray;
        //        break;
        //    case Rarity.Uncommon:
        //        _background.color = Color.green;
        //        break;
        //    case Rarity.Rare:
        //        _background.color = Color.blue;
        //        break;
        //    case Rarity.Epic:
        //        _background.color = Color.yellow;
        //        break;
        //    case Rarity.Legendary:
        //        _background.color = Color.magenta;
        //        break;
        //    case Rarity.Mythic:
        //        _background.color = Color.red;
        //        break;
        //    default:
        //        break;
        //}
    }

    public void _OnClick() {
            
    }

}
