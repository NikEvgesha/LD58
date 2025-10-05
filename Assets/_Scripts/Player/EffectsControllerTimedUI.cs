using UnityEngine;

public class EffectsControllerTimedUI : MonoBehaviour
{

    [Header("UI Overlays (full-screen Images with CanvasGroup)")]
    public CanvasGroup overlayDark;
    public CanvasGroup overlayRed;
    public CanvasGroup overlayFrost;
    public CanvasGroup overlayVignette;

    [Header("Camera shake")]
    public Transform cameraRoot;
    public float shakeRot = 10f;

    // --- Канал эффекта: плавное движение current -> target ---
    [System.Serializable]
    public class EffectChannel
    {
        [Range(0, 1)] public float current;   // фактическое значение
        [Range(0, 1)] public float target;    // целевое значение
        public float risePerSec = 2f;        // скорость нарастания
        public float fallPerSec = 1.5f;      // скорость спада

        public void Tick(float dt)
        {
            float speed = (target > current ? risePerSec : fallPerSec);
            current = Mathf.MoveTowards(current, target, speed * dt);
        }
    }

    [Header("Channels")]
    public EffectChannel dark = new EffectChannel() { risePerSec = 1.5f, fallPerSec = 1.5f };
    public EffectChannel red = new EffectChannel() { risePerSec = 8f, fallPerSec = 2.5f };
    public EffectChannel frost = new EffectChannel() { risePerSec = 2.5f, fallPerSec = 1.5f };
    public EffectChannel vign = new EffectChannel() { risePerSec = 1.5f, fallPerSec = 1.5f };

    // Доп. параметры «кровавых импульсов» (урон)
    [Header("Red decay")]
    public float redImpulseDecayPerSec = 1.5f; // как быстро цель убывает сама

    // Shake state
    Vector3 basePos; Quaternion baseRot;
    float shakeT, shakeDur, shakeMag;

    void Awake()
    {
        if (G.EffectsControllerTimedUI != null && G.EffectsControllerTimedUI != this) { Destroy(gameObject); return; }
        G.EffectsControllerTimedUI = this;

        if (!cameraRoot && Camera.main) cameraRoot = Camera.main.transform;
        if (cameraRoot) { basePos = cameraRoot.localPosition; baseRot = cameraRoot.localRotation; }

        // Стартовые альфы
        SetAlpha(overlayDark, 0); SetAlpha(overlayRed, 0);
        SetAlpha(overlayFrost, 0); SetAlpha(overlayVignette, 0);
    }

    void OnDestroy() { if (G.EffectsControllerTimedUI == this) G.EffectsControllerTimedUI = null; }

    void Update()
    {
        float dt = Time.deltaTime;

        // Плавное движение каналов к target
        dark.Tick(dt); red.Tick(dt); frost.Tick(dt); vign.Tick(dt);

        // Мягкое «самозатухание» для красноты: target постепенно снижается
        if (red.target > 0f)
        {
            red.target = Mathf.Max(0f, red.target - redImpulseDecayPerSec * dt);
        }
        /*if (frost.target > 0f)
        {
            frost.target = Mathf.Max(0f, frost.target - redImpulseDecayPerSec * dt);
        }*/

        // Применяем к UI
        SetAlpha(overlayDark, dark.current);
        SetAlpha(overlayRed, red.current);
        SetAlpha(overlayFrost, frost.current);
        SetAlpha(overlayVignette, vign.current);

        // Shake
        if (shakeT > 0f && cameraRoot)
        {
            shakeT -= dt;
            float p = Mathf.Clamp01(shakeT / shakeDur);
            float amp = shakeMag * p;
            cameraRoot.localPosition = basePos + new Vector3(
                (Random.value * 2 - 1f) * amp, (Random.value * 2 - 1f) * amp, 0f);
            cameraRoot.localRotation = Quaternion.Euler(0, 0, (Random.value * 2 - 1f) * amp * shakeRot);
            if (shakeT <= 0f)
            {
                cameraRoot.localPosition = basePos;
                cameraRoot.localRotation = baseRot;
            }
        }
    }

    static void SetAlpha(CanvasGroup cg, float a) { if (cg) cg.alpha = Mathf.Clamp01(a); }

    // ---------- ПУБЛИЧНОЕ API ----------
    // Потемнение/виньетка — задаём целевое значение, контроллер сам доведёт current
    public void SetDarkTarget(float t) { dark.target = Mathf.Clamp01(t); }
    public void SetVignetteTarget(float t) { vign.target = Mathf.Clamp01(t); }

    // Замерзание — то же (под «взглядом» держим 1, без взгляда — 0)
    public void SetFreezeTarget(float t) { frost.target = Mathf.Clamp01(t); }
    public void AddFreezeImpulse(float amount) 
    {
        frost.target = Mathf.Clamp01(frost.target + Mathf.Max(0f, amount));
    }

    // Красный «урон» — импульс: увеличиваем target, но не перезапускаем анимацию
    public void AddRedImpulse(float amount)
    {
        red.target = Mathf.Clamp01(red.target + Mathf.Max(0f, amount));
    }

    // Настройка скоростей на лету (по желанию)
    public void SetFreezeSpeeds(float rise, float fall) { frost.risePerSec = rise; frost.fallPerSec = fall; }
    public void SetRedSpeeds(float rise, float fall) { red.risePerSec = rise; red.fallPerSec = fall; }

    // Шейк
    public void Shake(float magnitude, float duration)
    {
        shakeMag = magnitude; shakeDur = Mathf.Max(0.01f, duration); shakeT = shakeDur;
    }
}
