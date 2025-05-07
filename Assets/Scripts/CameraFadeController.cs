using System;
using System.Collections;
using UnityEngine;

public class CameraFadeController : MonoBehaviour
{
    public float thresholdY = -15f;
    public float fadeSpeed = 1f;
    private float _alpha;
    private Coroutine _currentTween;

    private void Update()
    {
        var target = transform.position.y <= thresholdY ? 1f : 0f;
        if (Mathf.Approximately(target, _alpha)) return;
        if (_currentTween != null) StopCoroutine(_currentTween);
        _currentTween = StartCoroutine(FadeTo(target));
    }

    public event Action<float> OnFadeValueChanged;

    private IEnumerator FadeTo(float targetAlpha)
    {
        while (!Mathf.Approximately(_alpha, targetAlpha))
        {
            _alpha = Mathf.MoveTowards(_alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            OnFadeValueChanged?.Invoke(_alpha);
            yield return null;
        }

        _currentTween = null;
    }
}