using UnityEngine;
using System.Collections;

public class EffectsControllerTimedUI : MonoBehaviour
{
    [Header("UI Overlays (assign CanvasGroup on full-screen Images)")]
    public CanvasGroup overlayDark;   // чёрный фуллскрин
    public CanvasGroup overlayRed;    // красный фуллскрин (удар/урон)
    public CanvasGroup overlayFrost;  // текстура инея по краям (PNG с альфой)
    public CanvasGroup overlayVignette; // виньетка-градиент (опционально, PNG)

    [Header("Camera shake")]
    public Transform cameraRoot;    // камера или риг, который будем шатать
    public float shakeRot = 10f;

    Vector3 basePos;
    Quaternion baseRot;

    void Awake()
    {
        if (G.EffectsControllerTimedUI != null && G.EffectsControllerTimedUI != this) { Destroy(gameObject); return; }
        G.EffectsControllerTimedUI = this;

        if (!cameraRoot && Camera.main) cameraRoot = Camera.main.transform;
        if (cameraRoot)
        {
            basePos = cameraRoot.localPosition;
            baseRot = cameraRoot.localRotation;
        }

        // гарантия стартовых альф
        SetAlpha(overlayDark, 0);
        SetAlpha(overlayRed, 0);
        SetAlpha(overlayFrost, 0);
        SetAlpha(overlayVignette, 0);
    }

    void OnDestroy() { if (G.EffectsControllerTimedUI == this) G.EffectsControllerTimedUI = null; }

    // ---------- PUBLIC API (вызывай из геймплея) ----------
    public void Darken(float strength, float duration) => StartCoroutine(FadePulse(overlayDark, strength, duration));
    public void Redness(float strength, float duration) => StartCoroutine(FadePulse(overlayRed, strength, duration));
    public void Freeze(float strength, float duration) => StartCoroutine(FadePulse(overlayFrost, strength, duration));
    public void Vignette(float strength, float duration) => StartCoroutine(FadePulse(overlayVignette, strength, duration));
    public void Shake(float magnitude, float duration) => StartCoroutine(ShakeRoutine(magnitude, duration));

    // ---------- CORE ROUTINES ----------
    IEnumerator FadePulse(CanvasGroup cg, float strength, float duration)
    {
        if (!cg) yield break;
        strength = Mathf.Clamp01(strength);
        duration = Mathf.Max(0.01f, duration);

        float half = duration * 0.5f;
        float t = 0f;

        // Вход (0 -> strength)
        float start = cg.alpha;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = t / half;
            cg.alpha = Mathf.Lerp(start, strength, p);
            yield return null;
        }

        // Выход (strength -> 0)
        t = 0f;
        start = cg.alpha;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = t / half;
            cg.alpha = Mathf.Lerp(start, 0f, p);
            yield return null;
        }
        cg.alpha = 0f;
    }

    IEnumerator ShakeRoutine(float magnitude, float duration)
    {
        if (!cameraRoot) yield break;
        float t = duration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            float p = t / duration;
            float amp = magnitude * p; // затухание

            cameraRoot.localPosition = basePos + new Vector3(
                (Random.value * 2f - 1f) * amp,
                (Random.value * 2f - 1f) * amp,
                0f);

            cameraRoot.localRotation = Quaternion.Euler(0f, 0f, (Random.value * 2f - 1f) * amp * shakeRot);
            yield return null;
        }
        cameraRoot.localPosition = basePos;
        cameraRoot.localRotation = baseRot;
    }

    static void SetAlpha(CanvasGroup cg, float a)
    {
        if (cg) cg.alpha = Mathf.Clamp01(a);
    }
}
