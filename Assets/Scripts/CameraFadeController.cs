using System;
using System.Collections;
using UnityEngine;

public class CameraFadeController : MonoBehaviour
{
    [Header("Position Threshold")]
    [Tooltip("Y position below which the screen will fade to white.")]
    public float thresholdY = -15f;

    [Header("Fade Settings")]
    [Tooltip("Default duration (in seconds) for fade in/out if no duration is specified.")]
    public float defaultFadeDuration = 1f;

    /// <summary>
    /// Current alpha value between 0 (transparent) and 1 (opaque).
    /// </summary>
    public float Alpha { get; private set; }

    private Coroutine _currentTween;
    public event Action<float> OnFadeValueChanged;

    private void Update()
    {
        // Determine target alpha based on Y position
        float target = transform.position.y <= thresholdY ? 1f : 0f;

        // If already at target, do nothing
        if (Mathf.Approximately(target, Alpha))
            return;

        // Stop any running tween
        if (_currentTween != null)
            StopCoroutine(_currentTween);

        // Start fade with default duration
        _currentTween = StartCoroutine(FadeTo(target, defaultFadeDuration));
    }

    /// <summary>
    /// Fades the screen alpha from its current value to targetAlpha over duration seconds.
    /// </summary>
    /// <param name="targetAlpha">Desired final alpha (0 to 1).</param>
    /// <param name="duration">Time in seconds for the fade.</param>
    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = Alpha;
        float elapsed = 0f;

        // Avoid division by zero
        if (duration <= 0f)
        {
            Alpha = targetAlpha;
            OnFadeValueChanged?.Invoke(Alpha);
            _currentTween = null;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            OnFadeValueChanged?.Invoke(Alpha);
            yield return null;
        }

        // Ensure exact final value
        Alpha = targetAlpha;
        OnFadeValueChanged?.Invoke(Alpha);
        _currentTween = null;
    }

    /// <summary>
    /// Public API: Fade the screen to white over a specified duration.
    /// </summary>
    /// <param name="duration">Time in seconds for the fade.</param>
    public void FadeToWhite(float duration)
    {
        if (_currentTween != null)
            StopCoroutine(_currentTween);

        _currentTween = StartCoroutine(FadeTo(1f, duration));
    }

    /// <summary>
    /// Public API: Fade the screen from white (opaque) back to transparent over a specified duration.
    /// </summary>
    /// <param name="duration">Time in seconds for the fade.</param>
    public void FadeFromWhite(float duration)
    {
        if (_currentTween != null)
            StopCoroutine(_currentTween);

        _currentTween = StartCoroutine(FadeTo(0f, duration));
    }
}