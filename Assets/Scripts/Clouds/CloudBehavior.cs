using UnityEngine;
using System.Collections;

public class CloudBehavior : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private float timer;
    private bool initialized;

    private Vector3 originalScale;
    private float fadeDuration = 1.5f; // Duration of fade-out at end of life

    public void Initialize(Vector3 dir, float speed, float sizeMod, bool reverseDirection, float lifetime)
    {
        this.direction = ((reverseDirection ? -dir : dir)).normalized;
        this.speed = speed;
        this.lifetime = lifetime;
        this.timer = 0f;

        originalScale = Vector3.one * sizeMod;
        transform.localScale = originalScale;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        transform.position += direction * (speed * Time.deltaTime);
        timer += Time.deltaTime;

        float timeLeft = lifetime - timer;

        // Start scaling down if we're in the fade period
        if (timeLeft <= fadeDuration)
        {
            float t = Mathf.Clamp01(1 - (timeLeft / fadeDuration)); // goes from 0 to 1
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
        }

        if (timer >= lifetime)
        {
            gameObject.SetActive(false);
            initialized = false;
        }
    }
}