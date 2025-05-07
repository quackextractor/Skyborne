using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFaderUI : MonoBehaviour
{
    public CameraFadeController controller;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (controller != null)
            controller.OnFadeValueChanged += SetAlpha;
    }

    private void OnDisable()
    {
        if (controller != null)
            controller.OnFadeValueChanged -= SetAlpha;
    }

    private void SetAlpha(float a)
    {
        _canvasGroup.alpha = a;
    }
}