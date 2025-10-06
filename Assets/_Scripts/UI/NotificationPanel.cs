using UnityEngine;

public class NotificationPanel : MonoBehaviour
{
    [SerializeField] private ItemNotification _newItemNotification;

    private void Start()
    {
        G.Inventory.NewItem.AddListener(ShowNotification);
    }

    private void ShowNotification(CollectableItem item)
    {
        Instantiate(_newItemNotification, transform).Init(item.Data.Icon);
    }



}
