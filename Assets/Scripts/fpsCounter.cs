using TMPro;
using UnityEngine;

public class fpsCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    private float deltaTime;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        var fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
    }
}