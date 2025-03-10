using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Target : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 1f; // 100% default
    [SerializeField] private LayerMask platformEdgeLayer;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private LayerMask ragdollLayer;

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
        // Remove any rigidbody constraints
        _rb.constraints = RigidbodyConstraints.None;

        // Disable controlling components
        if (_isPlayer)
        {
            GetComponent<PlayerController>().enabled = false;
        }
        else
        {
            GetComponent<NavMeshAgent>().enabled = false;
            GetComponent<Enemy>().enabled = false; 
        }
        Debug.Log("Ragdoll enabled");

        // Enable physics simulation on the rigidbody
        _rb.isKinematic = false;
        
        // set to ragdoll layer to disable collision with other objects
        gameObject.layer = (int)Mathf.Log(ragdollLayer.value, 2);
        
        // Increase force impact and apply the knockback force
        force *= 1.5f;
        _rb.AddForce(knockbackDirection * force, ForceMode.Impulse);

        // Apply a random rotational force for added effect
        Vector3 randomTorque = new Vector3(
            Random.Range(-10f, 10f),
            Random.Range(5f, 15f),
            Random.Range(-10f, 10f)
        );

        // Ensure upward rotation by making the Y-component positive
        randomTorque.y = Mathf.Abs(randomTorque.y);
        _rb.AddTorque(randomTorque, ForceMode.Impulse);
    }


    public float GetAccumulatedKnockback() => _accumulatedKnockback;
    public void SetKnockbackMultiplier(float multiplier) => knockbackMultiplier = multiplier;
}