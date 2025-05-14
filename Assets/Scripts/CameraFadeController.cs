using System;
using System.Collections;
using UnityEngine;

public class CameraFadeController : MonoBehaviour
{
    public float thresholdY = -15f;
    public float fadeSpeed = 1f;
    public float victoryFadeDuration = 2f;
    public float Alpha { get; private set; }

    private Coroutine _currentTween;
    private Coroutine _manualFade;

    private bool _manualOverride = false;

    public event Action<float> OnFadeValueChanged;

    private void Awake()
    {
        Alpha = 1f;
        OnFadeValueChanged?.Invoke(Alpha);
    }

    private void OnEnable()
    {
        GameMaster.OnGameCompleted += OnGameCompleted;
    }

    private void OnDisable()
    {
        GameMaster.OnGameCompleted -= OnGameCompleted;
    }
    
    private void OnGameCompleted()
    {
        StartCoroutine(HandleGameCompletedCoroutine());
    }

    private void Update()
    {
        if (_manualOverride) return;

        var target = transform.position.y <= thresholdY ? 1f : 0f;
        if (Mathf.Approximately(target, Alpha)) return;

        if (_currentTween != null) StopCoroutine(_currentTween);
        _currentTween = StartCoroutine(FadeTo(target));
    }

    private IEnumerator HandleGameCompletedCoroutine()
    {
        yield return ManualFadeTo(1f, victoryFadeDuration);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
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
            while (!Mathf.Approximately(Alpha, targetAlpha))
            {
                Alpha = Mathf.MoveTowards(Alpha, targetAlpha, fadeSpeed * Time.deltaTime);
                OnFadeValueChanged?.Invoke(Alpha);
                yield return null;
            }
        }
        else
        {
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
        _manualOverride = Mathf.Approximately(targetAlpha, 1f);

        _manualFade = null;
    }

    public void FadeToWhite(float duration = -1)
    {
        if (_currentTween != null) StopCoroutine(_currentTween);
        if (_manualFade != null) StopCoroutine(_manualFade);

        _manualOverride = true;
        _manualFade = StartCoroutine(ManualFadeTo(1f, duration));
    }

    public void FadeFromWhite(float duration = -1)
    {
        if (_currentTween != null) StopCoroutine(_currentTween);
        if (_manualFade != null) StopCoroutine(_manualFade);

        _manualFade = StartCoroutine(ManualFadeTo(0f, duration));
    }
}
