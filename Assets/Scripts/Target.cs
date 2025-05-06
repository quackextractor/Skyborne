using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Target : MonoBehaviour
{
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 1f; // 100% default
    [SerializeField] private LayerMask platformEdgeLayer;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField, Tooltip("Select exactly one layer here.")] private int ragdollLayerIndex = 6;

  /*  [Header("Ragdoll Launch Settings")]
    [Tooltip("0 = flat (no lift), 1 = 45° upwards")] 
    [SerializeField, Range(-1f, 1f)] */ private float upwardFactor = -0.01f;

    private const float InitialAccumulatedKnockback = 1f;
    public float _accumulatedKnockback = InitialAccumulatedKnockback;

    private Enemy _enemy;
    private bool _hasEnemyScript;
    private bool _hasHealthText;
    private bool _isPlayer;

    private Rigidbody _rb;
    private Renderer _renderer;

    // Root colliders/controllers
    private CapsuleCollider _rootCapsule;
    private CharacterController _charController;

    // Ragdoll parts
    private List<Collider> _ragdollColliders;
    private List<Rigidbody> _ragdollRigidbodies;

    public float AccumulatedKnockback { get => _accumulatedKnockback; set => _accumulatedKnockback = value; }

    private void Awake()
    {
        // Core components
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        _enemy = GetComponent<Enemy>();
        _isPlayer = CompareTag("Player");
        _hasHealthText = playerHealthText != null;
        _hasEnemyScript = _enemy != null;

        if (_rb == null) Debug.LogError("Target requires a Rigidbody component!");
        if (_isPlayer && playerHealthText == null)
            Debug.LogWarning("Player Target has an unassigned playerHealthText!");

        // Root collider/controller references
        _rootCapsule    = GetComponent<CapsuleCollider>();
        _charController = GetComponent<CharacterController>();

        // Collect all child ragdoll parts (exclude this root rigidbody)
        _ragdollColliders   = new List<Collider>();
        _ragdollRigidbodies = new List<Rigidbody>();
        foreach (var childRb in GetComponentsInChildren<Rigidbody>())
        {
            if (childRb == _rb) continue;
            _ragdollRigidbodies.Add(childRb);
            var col = childRb.GetComponent<Collider>();
            if (col != null) _ragdollColliders.Add(col);
        }

        // Disable all ragdoll colliders & physics until needed
        foreach (var col in _ragdollColliders)
            col.enabled = false;
        foreach (var rb in _ragdollRigidbodies)
            rb.isKinematic = true;
    }

    private void Update()
    {
        if (_isPlayer && _hasHealthText)
            playerHealthText.text = "Current Knockback: " + _accumulatedKnockback;
    }

    public void TakeAttack(Attack attack)
    {
        Vector3 dir = (transform.position - attack.originPosition).normalized;
        float totalKb = attack.knockbackValue * _accumulatedKnockback * knockbackMultiplier;
        _accumulatedKnockback = Mathf.Min(5f, _accumulatedKnockback + attack.damageValue / 100f);

        ApplyKnockbackForce(dir, totalKb);

        if (!_isPlayer) StartCoroutine(FlashEffect());
    }

    public void TakeAttack(Vector3 attackDir, float damage, float knockback)
    {
        _accumulatedKnockback = Mathf.Min(5f, _accumulatedKnockback + damage / 100f);
        float totalKb = knockback * _accumulatedKnockback * knockbackMultiplier;

        ApplyKnockbackForce(attackDir.normalized, totalKb);

        if (!_isPlayer) StartCoroutine(FlashEffect());
    }

    private IEnumerator FlashEffect()
    {
        if (_hasEnemyScript)
        {
            _renderer.material.SetColor(BaseColor, Color.white);
            yield return new WaitForSeconds(0.1f);
            _renderer.material.SetColor(BaseColor, _enemy.BaseColor);
        }
    }

    private void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayerRecursively(child, layer);
    }

    public void ApplyKnockbackForce(Vector3 direction, float force)
    {
        if (_rb == null) return;

        if (CheckForFallOff(direction, force))
            EnableRagdoll(direction, force);
        else
            _rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private bool CheckForFallOff(Vector3 dir, float force)
    {
        Vector3 origin = transform.position;
        Debug.DrawRay(origin, dir * force, Color.red, 1f);

        if (Physics.Raycast(origin, dir, out var hit, force, platformEdgeLayer))
        {
            Debug.Log("Platform edge detected. Switching to ragdoll.");
            return true;
        }
        return false;
    }

    private void EnableRagdoll(Vector3 dir, float force)
    {
        // 1) Remove constraints on root
        _rb.constraints = RigidbodyConstraints.None;

        // 2) Disable character scripts & controllers
        if (_isPlayer)
        {
            var component = GetComponent<PlayerController>();
            if (component != null) component.enabled = false;
        }
        else
        {
            foreach (var beh in GetComponents<Behaviour>())
                if (beh != this) beh.enabled = false;
            if (_rootCapsule    != null) _rootCapsule.enabled    = false;
            if (_charController != null) _charController.enabled = false;
        }

        // 3) Compute a true 3D launch direction
        Vector3 horiz = dir;
        horiz.y = 0;
        horiz.Normalize();
        Vector3 launchDir = (horiz + Vector3.up * upwardFactor).normalized;

        // 4) Activate ragdoll parts and give them the launch velocity
        foreach (var col in _ragdollColliders)
            col.enabled = true;

        foreach (var boneRb in _ragdollRigidbodies)
        {
            boneRb.isKinematic = false;
            boneRb.velocity = launchDir * force * 1.5f;
            boneRb.angularVelocity = Random.onUnitSphere * force * 0.1f;
        }

        // 5) Switch the root to ragdoll too
        _rb.isKinematic = false;
        SetLayerRecursively(transform, ragdollLayerIndex);

        // 6) (Optional) also launch the root a bit
        _rb.velocity = launchDir * force * 1.5f;
        _rb.angularVelocity = Random.onUnitSphere * force * 0.2f;

        Debug.Log("Ragdoll launched!");
        
        // 7) Schedule enemy destruction after 10 seconds
        if (!_isPlayer){
            Destroy(gameObject, 10f);
        }
    }

    public void ResetAccumulatedKnockback()
    {
        _accumulatedKnockback = InitialAccumulatedKnockback;
        if (_isPlayer && _hasHealthText)
            playerHealthText.text = "Current Knockback: " + _accumulatedKnockback;
    }

    public float GetAccumulatedKnockback() => _accumulatedKnockback;
    public void SetKnockbackMultiplier(float m) => knockbackMultiplier = m;
}
