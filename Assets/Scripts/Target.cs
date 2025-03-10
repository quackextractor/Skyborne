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
                EnableRagdoll(direction, force);
            }
            else
            {
                _rb.AddForce(direction * force, ForceMode.Impulse);
            }
        }
    }

    private bool CheckForFallOff(Vector3 knockbackDirection, float force)
    {
        float rayLength = force;
        Vector3 rayOrigin = transform.position;

        Debug.DrawRay(rayOrigin, knockbackDirection * rayLength, Color.red, 1f);

        if (Physics.Raycast(rayOrigin, knockbackDirection, out RaycastHit hit, rayLength, platformEdgeLayer))
        {
            Debug.Log("Platform edge detected within knockback range. Initiating fall-off behavior.");
            return true;
        }
        return false;
    }

    private void EnableRagdoll(Vector3 knockbackDirection, float force)
    {
        // Add additional ragdoll components/force application here
        _rb.constraints = RigidbodyConstraints.None;
        if (_isPlayer)
        {
            this.GetComponent<PlayerController>().enabled = false;
        }
        Debug.Log("Ragdoll enabled");

        _rb.isKinematic = false;
    
        // Increase force to be more impactful
        force *= 1.5f;
        // Apply the knockback force
        _rb.AddForce(knockbackDirection * force, ForceMode.Impulse);

        // Apply random rotational force (torque)
        Vector3 randomTorque = new Vector3(
            Random.Range(-10f, 10f),
            Random.Range(5f, 15f),  
            Random.Range(-10f, 10f)
        );

        // Ensure that the rotation is 45° upwards (so a small Y-component is added to randomTorque)
        randomTorque.y = Mathf.Abs(randomTorque.y); // Positive upward rotational force
    
        _rb.AddTorque(randomTorque, ForceMode.Impulse);
    
        if (GetComponent<Enemy>() != null)
        {
            // Disable AI
            GetComponent<Enemy>().enabled = false; 
        }
    }


    public float GetAccumulatedKnockback() => _accumulatedKnockback;
    public void SetKnockbackMultiplier(float multiplier) => knockbackMultiplier = multiplier;
}