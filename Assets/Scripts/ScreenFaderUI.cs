using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFaderUI : MonoBehaviour
{
    public CameraFadeController controller;
    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    void OnEnable()
    {
        if (controller != null)
            controller.OnFadeValueChanged += SetAlpha;
    }

    void OnDisable()
    {
        if (controller != null)
            controller.OnFadeValueChanged -= SetAlpha;
    }

    void SetAlpha(float a) => _canvasGroup.alpha = a;
}