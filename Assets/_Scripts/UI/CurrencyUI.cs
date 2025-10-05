using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    private TextMeshProUGUI _amountText;

    private void Awake()
    {
        _amountText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        G.Currency.CurrencyChanged.AddListener(UpdateUI);
    }

    private void Start()
    {
        UpdateUI(G.Currency.Coins);
    }

    private void UpdateUI(int amount)
    {
        _amountText.text = amount.ToString();
    }
}
