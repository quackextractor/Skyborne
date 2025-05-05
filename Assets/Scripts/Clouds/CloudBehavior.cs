using UnityEngine;

public class CloudBehavior : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private float timer;
    private bool initialized;

    private Vector3 originalScale;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    public void Initialize(Vector3 dir, float speed, float sizeMod, bool reverseDirection, float lifetime)
    {
        direction = ((reverseDirection ? -dir : dir)).normalized;
        this.speed = speed;
        this.lifetime = lifetime;
        timer = 0f;

        originalScale = Vector3.one * sizeMod;
        transform.localScale = Vector3.zero; // Start invisible

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        transform.position += direction * (speed * Time.deltaTime);
        timer += Time.deltaTime;

        float timeLeft = lifetime - timer;

        // --- Fade In ---
        if (timer < fadeInDuration)
        {
            float t = Mathf.Clamp01(timer / fadeInDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
        }
        // --- Fade Out ---
        else if (timeLeft <= fadeOutDuration)
        {
            float t = Mathf.Clamp01(1 - (timeLeft / fadeOutDuration));
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
        }
        else
        {
            // Maintain full scale
            transform.localScale = originalScale;
        }

        if (timer >= lifetime)
        {
            gameObject.SetActive(false);
            initialized = false;
        }
    }
}