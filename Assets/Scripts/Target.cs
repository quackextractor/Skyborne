using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 1f; // 100% default
    private float _accumulatedKnockback = 1f;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("Target requires a Rigidbody component!");
        }
    }

    public void TakeAttack(Attack attack)
    {
        Vector3 knockbackDirection = (transform.position - attack.originPosition).normalized;
        
        float totalKnockback = attack.knockbackValue * _accumulatedKnockback;
        totalKnockback *= knockbackMultiplier;
        
        _accumulatedKnockback = Mathf.Min(5f, _accumulatedKnockback + Mathf.Log(attack.damageValue + 1, 2));
        
        ApplyKnockbackForce(knockbackDirection, totalKnockback);
    }

    private void ApplyKnockbackForce(Vector3 direction, float force)
    {
        if (_rb != null)
        {
            _rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }
    
    public float GetAccumulatedKnockback() => _accumulatedKnockback;
    public void SetKnockbackMultiplier(float multiplier) => knockbackMultiplier = multiplier;
}
