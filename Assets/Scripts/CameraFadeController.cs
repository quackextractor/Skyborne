using System;
using System.Collections;
using UnityEngine;

public class CameraFadeController : MonoBehaviour
{
    public float thresholdY = -15f;
    public float fadeSpeed = 1f;
    public float Alpha { get; private set; }
    private Coroutine _currentTween;

    private void Update()
    {
        var target = transform.position.y <= thresholdY ? 1f : 0f;
        if (Mathf.Approximately(target, Alpha)) return;
        if (_currentTween != null) StopCoroutine(_currentTween);
        _currentTween = StartCoroutine(FadeTo(target));
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
    
    public void FadeToWhite()
    {
        if (_currentTween != null) StopCoroutine(_currentTween);
        _currentTween = StartCoroutine(FadeTo(1f));
    }

    public void FadeFromWhite()
    {
        if (_currentTween != null) StopCoroutine(_currentTween);
        _currentTween = StartCoroutine(FadeTo(0f));
    }
}