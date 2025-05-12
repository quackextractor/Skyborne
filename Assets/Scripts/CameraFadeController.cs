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

    private void Update()
    {
        // Only handle automatic fade based on position if there's no manual fade happening
        if (_manualFade == null)
        {
            var target = transform.position.y <= thresholdY ? 1f : 0f;
            if (Mathf.Approximately(target, Alpha)) return;

            if (_currentTween != null) StopCoroutine(_currentTween);
            _currentTween = StartCoroutine(FadeTo(target));
        }
    }

    public event Action<float> OnFadeValueChanged;

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
        
        // Use default fade speed if no duration is specified
        if (duration <= 0)
        {
            while (!Mathf.Approximately(Alpha, targetAlpha))
            {
                Alpha = Mathf.MoveTowards(Alpha, targetAlpha, fadeSpeed * Time.deltaTime);
                OnFadeValueChanged?.Invoke(Alpha);
                yield return null;
            }
        }
        else
        {
            // Use specified duration
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                OnFadeValueChanged?.Invoke(Alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        Alpha = targetAlpha;
        OnFadeValueChanged?.Invoke(Alpha);
        _manualFade = null;
    }

    public void FadeToWhite(float duration = -1)
    {
        // Stop any current transitions
        if (_currentTween != null) StopCoroutine(_currentTween);
        if (_manualFade != null) StopCoroutine(_manualFade);

        _manualFade = StartCoroutine(ManualFadeTo(1f, duration));
    }

    public void FadeFromWhite(float duration = -1)
    {
        // Stop any current transitions
        if (_currentTween != null) StopCoroutine(_currentTween);
        if (_manualFade != null) StopCoroutine(_manualFade);

        _manualFade = StartCoroutine(ManualFadeTo(0f, duration));
    }
}