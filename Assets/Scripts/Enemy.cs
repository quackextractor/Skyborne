using System;
using ScriptObj;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // Event invoked whenever a new Enemy is spawned
    public static event Action OnEnemySpawned;

    private static readonly int BaseColor1 = Shader.PropertyToID("_BaseColor");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    [SerializeField] private EnemyStats stats;
    [SerializeField] private AudioClip deathAudioClip;
    [SerializeField] private Weapon weapon;
    [SerializeField] private float spawnDelay = 1f; // Duration to disable movement/attack after spawn

    private NavMeshAgent _agent;
    private float _attackCooldown;
    private float _spawnTimer;
    private Transform _player;
    private Target _targetComponent;
    private Renderer _enemyRenderer;
    private Color _originalColor;
    private Animator _anim;

    public Color BaseColor { get; private set; }
    public EnemyStats Stats => stats;

    public AudioClip DeathAudioClip { get => deathAudioClip; set => deathAudioClip = value; }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false; // disable built-in turning
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _targetComponent = GetComponent<Target>();
        _enemyRenderer = GetComponent<Renderer>();
        _anim = GetComponent<Animator>();

        if (_enemyRenderer != null)
            _originalColor = _enemyRenderer.material.GetColor(BaseColor1);
    }

    private void Update()
    {
        if (_spawnTimer > 0f)
        {
            _spawnTimer -= Time.deltaTime;
            _agent.isStopped = true;
            return;
        }

        if (_attackCooldown > 0f)
            _attackCooldown -= Time.deltaTime;

        UpdateBurningVisuals();

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distanceToPlayer <= stats.range)
        {
            _agent.isStopped = true;
            RotateTowardsPlayer();
            TryAttack();
            return;
        }

        var isWalking = _anim.GetBool(IsWalking);
        if (isWalking)
        {
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);
            }

            if (_agent.velocity.sqrMagnitude > 0.1f)
                RotateTowardsVelocity();
        }
        else
        {
            if (_agent.isOnNavMesh)
                _agent.isStopped = true;
        }
    }

    private void UpdateBurningVisuals()
    {
        if (_targetComponent != null && _enemyRenderer != null)
        {
            if (_targetComponent.IsBurning)
            {
                Color burningColor = new Color(1.0f, 0.3f, 0.3f);
                _enemyRenderer.material.SetColor(BaseColor1, Color.Lerp(_originalColor, burningColor, 0.6f));
            }
            else
            {
                _enemyRenderer.material.SetColor(BaseColor1, _originalColor);
            }
        }
    }

    public void Setup(EnemyStats newStats)
    {
        stats = newStats;
        InitializeStats();

        if (_agent != null)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
                _agent.Warp(hit.position);
            else
                Debug.LogWarning("Enemy not near a valid NavMesh area after spawn. Warp failed.");
        }

        _spawnTimer = spawnDelay; // Disable movement/attacks temporarily after spawn

        OnEnemySpawned?.Invoke();
    }

    private void InitializeStats()
    {
        _agent.speed = stats.movementSpeed;
        _agent.angularSpeed = stats.turnSpeed;
        _agent.acceleration = stats.acceleration;

        weapon.Damage = stats.weaponDamage;
        weapon.Knockback = stats.weaponKnockback;

        if (_enemyRenderer != null)
        {
            _enemyRenderer.material.SetColor(BaseColor1, stats.variantColor);
            _originalColor = stats.variantColor;
        }

        BaseColor = stats.variantColor;
    }

    private void RotateTowardsPlayer()
    {
        var dir = _player.position - transform.position;
        dir.y = 0;
        var target = Quaternion.LookRotation(dir);
        var maxDelta = stats.turnSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, maxDelta);
    }

    private void RotateTowardsVelocity()
    {
        var dir = _agent.velocity;
        dir.y = 0;
        var target = Quaternion.LookRotation(dir.normalized);
        var maxDelta = stats.turnSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, maxDelta);
    }

    private void TryAttack()
    {
        if (_attackCooldown <= 0)
        {
            weapon.StartAttack();
            _attackCooldown = 1f / stats.attackSpeed;
        }
    }
}
