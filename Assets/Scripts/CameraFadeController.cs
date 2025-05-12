using System;
using System.Collections;
using UnityEngine;

public class CameraFadeController : MonoBehaviour
{
    public float thresholdY = -15f;
    public float fadeSpeed = 1f;
    public float Alpha { get; private set; }

    private Coroutine _currentTween;
    private Coroutine _manualFade;
    
    // When true, we suppress automatic fades until we FadeFromWhite()
    private bool _manualOverride = false;

    public event Action<float> OnFadeValueChanged;

    private void Update()
    {
        // Only handle automatic fades if we're not in manual override
        if (_manualOverride) return;

        var target = transform.position.y <= thresholdY ? 1f : 0f;
        if (Mathf.Approximately(target, Alpha)) return;

        if (_currentTween != null) StopCoroutine(_currentTween);
        _currentTween = StartCoroutine(FadeTo(target));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        while (!Mathf.Approximately(Alpha, targetAlpha))
        {
            Alpha = Mathf.MoveTowards(Alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            OnFadeValueChanged?.Invoke(Alpha);
            yield return null;
        }
        _currentTween = null;
    }

    private IEnumerator ManualFadeTo(float targetAlpha, float duration = -1)
    {
        float startAlpha = Alpha;
        float elapsed = 0f;

        if (duration <= 0)
        {
            // default-speed fade
            while (!Mathf.Approximately(Alpha, targetAlpha))
            {
                Alpha = Mathf.MoveTowards(Alpha, targetAlpha, fadeSpeed * Time.deltaTime);
                OnFadeValueChanged?.Invoke(Alpha);
                yield return null;
            }
        }
        else
        {
            // timed fade
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                OnFadeValueChanged?.Invoke(Alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Ensure final value
        Alpha = targetAlpha;
        OnFadeValueChanged?.Invoke(Alpha);

        // Maintain manual override if we just faded to white, clear if we faded back
        _manualOverride = Mathf.Approximately(targetAlpha, 1f);

        _manualFade = null;
    }

    public void FadeToWhite(float duration = -1)
    {
        // stop any active fades
        if (_currentTween != null) StopCoroutine(_currentTween);
        if (_manualFade != null) StopCoroutine(_manualFade);

        // enter manual override and start the fade-to-white
        _manualOverride = true;
        _manualFade = StartCoroutine(ManualFadeTo(1f, duration));
    }

    public void FadeFromWhite(float duration = -1)
    {
        // stop any active fades
        if (_currentTween != null) StopCoroutine(_currentTween);
        if (_manualFade != null) StopCoroutine(_manualFade);

        // start fade-from-white (ManualFadeTo will clear _manualOverride when done)
        _manualFade = StartCoroutine(ManualFadeTo(0f, duration));
    }
}
