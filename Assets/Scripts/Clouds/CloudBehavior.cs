using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudBehavior : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private float timer;
    private bool initialized;

    /// <summary>
    /// Initializes cloud movement, size, direction, and lifetime.
    /// </summary>
    public void Initialize(Vector3 dir, float speed, float sizeMod, bool reverseDirection, float lifetime)
    {
        // Apply movement direction
        this.direction = ((reverseDirection ? -dir : dir)).normalized;
        this.speed = speed;
        this.lifetime = lifetime;
        this.timer = 0f;

        // Apply scale
        transform.localScale = Vector3.one * sizeMod;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        // Move cloud
        transform.position += direction * (speed * Time.deltaTime);

        // Update lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            gameObject.SetActive(false);
            initialized = false;
        }
    }
}