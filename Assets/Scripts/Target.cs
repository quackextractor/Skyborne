using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Target : MonoBehaviour
{
    private const float InitialAccumulatedKnockback = 1f;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private EnemyAnimationController _animController;
    private CurrencyManager _currencyManager;

    [field: Header("Knockback Settings")]
    [field: SerializeField]
    public float KnockbackMultiplier { get; set; } = 1f;

    [SerializeField] private LayerMask platformEdgeLayer;
    [SerializeField] private TextMeshProUGUI playerHealthText;

    [SerializeField] [Tooltip("Select exactly one layer here.")]
    private int ragdollLayerIndex = 6;

    [FormerlySerializedAs("_accumulatedKnockback")]
    public float accumulatedKnockback = InitialAccumulatedKnockback;

    [Header("Burning Effect")]
    private bool _isBurning;
    private float _burningTimeRemaining;
    private ParticleSystem _fireEffect;
    private Coroutine _burningCoroutine;
    private const float UpwardFactor = -0.01f;
    private CharacterController _charController;

    private Enemy _enemy;
    private bool _hasEnemyScript;
    private bool _hasHealthText;
    private bool _isPlayer;

    // Ragdoll parts
    private List<Collider> _ragdollColliders;
    private List<Rigidbody> _ragdollRigidbodies;

    private Rigidbody _rb;
    private Renderer _renderer;

    // Root colliders/controllers
    private CapsuleCollider _rootCapsule;

    public float AccumulatedKnockback
    {
        get => accumulatedKnockback;
        set => accumulatedKnockback = value;
    }

    public bool IsBurning
    {
        get => _isBurning;
        private set => _isBurning = value;
    }

    private void Awake()
    {
        // Core components
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        _enemy = GetComponent<Enemy>();
        _isPlayer = CompareTag("Player");
        _hasHealthText = playerHealthText != null;
        _hasEnemyScript = _enemy != null;
        _currencyManager = FindObjectOfType<CurrencyManager>();

        if (_rb == null) Debug.LogError("Target requires a Rigidbody component!");
        if (_isPlayer && playerHealthText == null)
            Debug.LogWarning("Player Target has an unassigned playerHealthText!");

        // Root collider/controller references
        _rootCapsule = GetComponent<CapsuleCollider>();
        _charController = GetComponent<CharacterController>();

        // Collect all child ragdoll parts (exclude this root rigidbody)
        _ragdollColliders = new List<Collider>();
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

        if (!_isPlayer)
        {
            _animController = GetComponent<EnemyAnimationController>();
        }
    }

    private void Update()
    {
        if (_isPlayer && playerHealthText)
            playerHealthText.text = $"Current Knockback: {accumulatedKnockback*100}%";
    }

    private void OnDestroy()
    {
        if (_fireEffect != null)
        {
            Destroy(_fireEffect.gameObject);
        }
    }

    public void TakeAttack(Attack attack)
    {
        var dir = (transform.position - attack.originPosition).normalized;
        var totalKb = attack.knockbackValue * accumulatedKnockback * KnockbackMultiplier;
        accumulatedKnockback = Mathf.Min(5f, accumulatedKnockback + attack.damageValue / 100f);

        ApplyKnockbackForce(dir, totalKb);

        if (!_isPlayer)
        {
            StartCoroutine(FlashEffect());
            _animController?.Stun();
        }
    }

    public void TakeAttack(Vector3 attackDir, float damage, float knockback)
    {
        accumulatedKnockback = Mathf.Min(5f, accumulatedKnockback + damage / 100f);
        var totalKb = knockback * accumulatedKnockback * KnockbackMultiplier;

        ApplyKnockbackForce(attackDir.normalized, totalKb);

        if (!_isPlayer) StartCoroutine(FlashEffect());
    }

    private IEnumerator FlashEffect()
    {
        if (_hasEnemyScript && !_isBurning) // Don't flash if already burning
        {
            _renderer.material.SetColor(BaseColor, Color.white);
            yield return new WaitForSeconds(0.1f);
            _renderer.material.SetColor(BaseColor, _enemy.BaseColor);
        }
    }

    public void SetBurning(bool burning, float duration = 0f)
    {
        if (burning && !IsBurning)
        {
            IsBurning = true;
            _burningTimeRemaining = duration;
            if (_burningCoroutine != null) StopCoroutine(_burningCoroutine);
            _burningCoroutine = StartCoroutine(BurningProcess());
        }
        else if (!burning && IsBurning)
        {
            IsBurning = false;

            if (_fireEffect)
            {
                // stop particles
                var main = _fireEffect.main;
                _fireEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                // stop audio
                var audioSrc = _fireEffect.GetComponent<AudioSource>();
                if (audioSrc)
                    audioSrc.Stop();

                // destroy the whole GameObject after it's faded out
                float life = (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                    ? main.startLifetime.constantMax : main.startLifetime.constant;
                Destroy(_fireEffect.gameObject, main.duration + life);
                _fireEffect = null;
            }

            if (_burningCoroutine != null)
            {
                StopCoroutine(_burningCoroutine);
                _burningCoroutine = null;
            }
        }
    }
    
    public void SetFireEffect(ParticleSystem fireEffect)
    {
        if (_fireEffect != null)
            Destroy(_fireEffect.gameObject);
        _fireEffect = fireEffect;
    }

    private IEnumerator BurningProcess()
    {
        while (_burningTimeRemaining > 0f && IsBurning)
        {
            _burningTimeRemaining -= Time.deltaTime;
            yield return null;
        }
        SetBurning(false);
    }

    private void ApplyKnockbackForce(Vector3 direction, float force)
    {
        if (_rb == null) return;

        if (CheckForFallOff(direction, force))
            EnableRagdoll(direction, force);
        else
            _rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private bool CheckForFallOff(Vector3 dir, float force)
    {
        var origin = transform.position;
        Debug.DrawRay(origin, dir * force, Color.red, 1f);

        return Physics.Raycast(origin, dir, out var hit, force, platformEdgeLayer);
    }

    public void EnableRagdoll(Vector3 dir, float force)
    {
        _rb.constraints = RigidbodyConstraints.None;

        if (_isPlayer)
        {
            var component = GetComponent<PlayerController>();
            if (component != null) component.enabled = false;
        }
        else
        {
            // Notify GameMaster that this enemy has been defeated
            if (GameMaster.Instance != null)
            {
                GameMaster.Instance.OnEnemyDefeated();
            }

            if (_currencyManager != null)
            {
                _currencyManager.AddMoney(_enemy.Stats.moneyReward);
            }

            foreach (var beh in GetComponents<Behaviour>())
                if (beh != this)
                    beh.enabled = false;
            if (_rootCapsule != null) _rootCapsule.enabled = false;
            if (_charController != null) _charController.enabled = false;
        }

        var horiz = dir; horiz.y = 0; horiz.Normalize();
        var launchDir = (horiz + Vector3.up * UpwardFactor).normalized;

        foreach (var col in _ragdollColliders)
            col.enabled = true;

        foreach (var boneRb in _ragdollRigidbodies)
        {
            boneRb.isKinematic = false;
            boneRb.velocity = launchDir * force * 1.5f;
            boneRb.angularVelocity = Random.onUnitSphere * force * 0.1f;
        }

        _rb.isKinematic = false;
        SetLayerRecursively(transform, ragdollLayerIndex);

        _rb.velocity = launchDir * force * 1.5f;
        _rb.angularVelocity = Random.onUnitSphere * force * 0.2f;

        if (!_isPlayer) Destroy(gameObject, 10f);
    }

    private void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayerRecursively(child, layer);
    }

    public void ResetAccumulatedKnockback()
    {
        accumulatedKnockback = InitialAccumulatedKnockback;
        if (_isPlayer && _hasHealthText)
            playerHealthText.text = "Current Knockback: " + accumulatedKnockback;
    }

    public float GetAccumulatedKnockback()
    {
        return accumulatedKnockback;
    }

    public void SetKnockbackMultiplier(float m)
    {
        KnockbackMultiplier = m;
    }
}