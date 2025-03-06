using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 1f; // 100% default
    [SerializeField] private LayerMask platformEdgeLayer;

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
        
        _accumulatedKnockback = Mathf.Min(5f, _accumulatedKnockback + attack.damageValue / 100);
        
        ApplyKnockbackForce(knockbackDirection, totalKnockback);
    }

    private void ApplyKnockbackForce(Vector3 direction, float force)
    {
        if (_rb != null)
        {
            if (CheckForFallOff(direction, force))
            {
                
            }
            _rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    private bool CheckForFallOff(Vector3 knockbackDirection, float force)
    {
        if (!Physics.Raycast(transform.position, knockbackDirection, 2f, platformEdgeLayer))
        {
            EnableRagdoll(force);
            return true;
        }
        return false;
    }

    private void EnableRagdoll(float force)
    {
        _rb.isKinematic = false;
        _rb.AddForce(Vector3.down * force, ForceMode.Impulse);
        // Add additional ragdoll components/force application here
        GetComponent<Enemy>().enabled = false; // Disable AI
    }

    public float GetAccumulatedKnockback() => _accumulatedKnockback;
    public void SetKnockbackMultiplier(float multiplier) => knockbackMultiplier = multiplier;
}