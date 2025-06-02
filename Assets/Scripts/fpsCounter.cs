using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class FPSCounter : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("How often (in seconds) the FPS text is updated.")]
    [Range(0.1f, 2f)]
    public float updateInterval = 0.5f;

    [Tooltip("Show frame time in milliseconds alongside FPS.")]
    public bool showFrameTimeMs = true;

    [Tooltip("Automatically color-code the FPS text.")]
    public bool colorCodeFPS = true;

    [Header("Color Thresholds")]
    [Tooltip("FPS above which text is green.")]
    public float highFPSThreshold = 50f;
    [Tooltip("FPS above which text is yellow.")]
    public float mediumFPSThreshold = 30f;

    private TextMeshProUGUI _fpsText;
    private float _accumulatedTime = 0f;  // Sum of delta times
    private int _frames = 0;              // Frames drawn in the interval
    private float _timeLeft;              // Time left for current interval
    private StringBuilder _sb = new StringBuilder(64);

    private void Awake()
    {
        _fpsText = GetComponent<TextMeshProUGUI>();
        _timeLeft = updateInterval;
        
        // Apply saved visibility setting
        UpdateVisibility();
    }

    private void Start()
    {
        // Ensure visibility is correct at start
        UpdateVisibility();
    }

    private void Update()
    {
        // Only update if visible
        if (!_fpsText.enabled) return;
        
        // Accumulate time and frame count
        _timeLeft       -= Time.unscaledDeltaTime;
        _accumulatedTime += Time.unscaledDeltaTime;
        _frames++;

        // Update when interval has elapsed
        if (_timeLeft <= 0f)
        {
            float fps = _frames / _accumulatedTime;
            float ms  = _accumulatedTime * 1000f / _frames;

            // Build the display string
            _sb.Clear();
            _sb.Append("FPS: ").Append(Mathf.CeilToInt(fps));
            if (showFrameTimeMs)
            {
                _sb.Append(" (").Append(ms.ToString("F1")).Append(" ms)");
            }

            _fpsText.text = _sb.ToString();

            // Apply color-coding
            if (colorCodeFPS)
            {
                if (fps >= highFPSThreshold)
                    _fpsText.color = Color.green;
                else if (fps >= mediumFPSThreshold)
                    _fpsText.color = Color.yellow;
                else
                    _fpsText.color = Color.red;
            }

            // Reset for next interval
            _timeLeft         = updateInterval;
            _accumulatedTime = 0f;
            _frames          = 0;
        }
    }

    // Update visibility based on saved preference
    public void UpdateVisibility()
    {
        // PlayerPrefs key matches MenuController's ShowFPSKey
        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        
        // Enable both the Text component and parent Canvas
        _fpsText.enabled = showFPS;
        GetComponent<Canvas>().enabled = showFPS;
    }
}