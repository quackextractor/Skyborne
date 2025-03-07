using TMPro;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 1f; // 100% default
    [SerializeField] private LayerMask platformEdgeLayer;
    [SerializeField] private TextMeshProUGUI playerHealthText;

    private float _accumulatedKnockback = 1f;
    private Rigidbody _rb;  
    private bool _isPlayer;

    void Update()
    {
        if (_isPlayer && playerHealthText != null)
        {
            playerHealthText.text = "Current Knockback: " + _accumulatedKnockback;
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("Target requires a Rigidbody component!");
        }

        _isPlayer = gameObject.CompareTag("Player");

        if (_isPlayer && playerHealthText == null)
        {
            Debug.LogWarning("Player Target has an unassigned playerHealthText!");
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
            EnableRagdoll(knockbackDirection, force);
            return true;
        }
        return false;
    }

    private void EnableRagdoll(Vector3 knockbackDirection, float force)
    {
        _rb.isKinematic = false;
        _rb.AddForce(knockbackDirection * force, ForceMode.Impulse);
        // Add additional ragdoll components/force application here

        Debug.Log("Ragdoll enabled");
        
        if (GetComponent<Enemy>() != null)
        {
            // Disable AI
            GetComponent<Enemy>().enabled = false; 
        }
    }

    public float GetAccumulatedKnockback() => _accumulatedKnockback;
    public void SetKnockbackMultiplier(float multiplier) => knockbackMultiplier = multiplier;
}