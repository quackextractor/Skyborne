using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 1f; // 100% default
    private float accumulatedKnockback = 0f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Target requires a Rigidbody component!");
        }
    }

    public void TakeAttack(Attack attack)
    {
        // Calculate knockback direction
        Vector3 knockbackDirection = (transform.position - attack.originPosition).normalized;
        
        // Calculate total knockback
        float totalKnockback = attack.knockbackValue * knockbackMultiplier;
        accumulatedKnockback += totalKnockback;

        // Apply knockback force
        ApplyKnockbackForce(knockbackDirection, totalKnockback);
    }

    private void ApplyKnockbackForce(Vector3 direction, float force)
    {
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    // For external access if needed
    public float GetAccumulatedKnockback() => accumulatedKnockback;
    public void SetKnockbackMultiplier(float multiplier) => knockbackMultiplier = multiplier;
}
