using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultItemSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _amountText;
    [SerializeField] private GameObject _newIndicator;


    public void Init(CollectableItemData item, int amount, bool isNew=false)
    {
        _icon.sprite = item.Icon;
        _amountText.text = "x " + amount.ToString();
        _newIndicator.SetActive(isNew);
    }

}
