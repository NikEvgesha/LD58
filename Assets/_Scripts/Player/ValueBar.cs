using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class ValueBar : ManagedBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;                 // Image с Type=Filled (Fill Method = Horizontal)
    [SerializeField] private TextMeshProUGUI minText;         // Текст минимума
    [SerializeField] private TextMeshProUGUI maxText;         // Текст максимума
    [SerializeField] private TextMeshProUGUI valueText;       // (опционально) Текст текущего значения на баре

    [Header("Config")]
    [SerializeField] private float min = 0f;
    [SerializeField] private float max = 100f;
    [SerializeField] private float value = 100f;
    [SerializeField] private bool animate = false;
    [SerializeField] private float animateSpeed = 8f;         // чем больше, тем быстрее анимация
    [SerializeField] private string numberFormat = "0";       // формат чисел в текстах: "0", "0.0", "0.##" и т.п.

    // Внутреннее целевое значение для анимации
    private float targetValue;

    public float Min
    {
        get => min;
        set { min = value; ClampAll(); RefreshTexts(); RefreshFillImmediate(); }
    }

    public float Max
    {
        get => max;
        set { max = Mathf.Max(value, min + Mathf.Epsilon); ClampAll(); RefreshTexts(); RefreshFillImmediate(); }
    }

    public float Value
    {
        get => this.value;
        set
        {
            targetValue = Mathf.Clamp(value, min, max);
            if (!animate)
            {
                this.value = targetValue;
                RefreshFillImmediate();
                RefreshValueText();
            }
        }
    }

    private void Awake()
    {
        // Защита: если забыли выставить Image в Filled — сделаем сами.
        if (fillImage != null && fillImage.type != Image.Type.Filled)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
        targetValue = Mathf.Clamp(value, min, max);
        ClampAll();
        RefreshAllImmediate();
    }

    protected override void PausableUpdate()
    {
        if (!animate) return;

        if (!Mathf.Approximately(value, targetValue))
        {
            value = Mathf.Lerp(value, targetValue, Time.deltaTime * animateSpeed);

            // Чтобы не висеть в вечной асимптоте — снэп к цели.
            if (Mathf.Abs(value - targetValue) < 0.0001f)
                value = targetValue;

            RefreshFillImmediate();
            RefreshValueText();
        }
    }

    /// <summary>Полная инициализация.</summary>
    public void SetRange(float min, float max)
    {
        this.min = min;
        this.max = Mathf.Max(max, min + Mathf.Epsilon);
        ClampAll();
        RefreshAllImmediate();
    }

    /// <summary>Единовременное обновление всего (мин/макс/значение).</summary>
    public void Set(float value, float min = 0, float max = 100, bool animateToValue = false)
    {
        this.min = min;
        this.max = Mathf.Max(max, min + Mathf.Epsilon);
        animate = animateToValue;
        Value = value; // пройдет через сеттер с учетом animate
        RefreshTexts();
        if (!animate) RefreshAllImmediate();
    }

    /// <summary>Установить формат чисел для текстов, например "0" или "0.##".</summary>
    public void SetNumberFormat(string format)
    {
        numberFormat = string.IsNullOrEmpty(format) ? "0" : format;
        RefreshTexts();
        RefreshValueText();
    }

    private void ClampAll()
    {
        value = Mathf.Clamp(value, min, max);
        targetValue = Mathf.Clamp(targetValue, min, max);
    }

    private void RefreshAllImmediate()
    {
        RefreshTexts();
        RefreshFillImmediate();
        RefreshValueText();
    }

    private void RefreshTexts()
    {
        if (minText) minText.text = min.ToString(numberFormat);
        if (maxText) maxText.text = max.ToString(numberFormat);
    }

    private void RefreshValueText()
    {
        if (valueText) valueText.text = value.ToString(numberFormat);
    }

    private void RefreshFillImmediate()
    {
        if (!fillImage || max <= min) return;
        float t = Mathf.InverseLerp(min, max, value);
        fillImage.fillAmount = t;
    }
}
