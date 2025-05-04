using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private static readonly int BaseColor1 = Shader.PropertyToID("_BaseColor");

    [SerializeField] private EnemyStats stats;
    [SerializeField] private Weapon weapon;

    private NavMeshAgent _agent;
    private float _attackCooldown;
    private Transform _player;

    public Color BaseColor { get; private set; }
    public EnemyStats Stats => stats;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;           // disable built-in turning
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Setup(EnemyStats newStats)
    {
        stats = newStats;
        InitializeStats();

        // Warp onto NavMesh if needed
        if (_agent != null)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                _agent.Warp(hit.position);
            else
                Debug.LogWarning("Enemy not near a valid NavMesh area after spawn. Warp failed.");
        }
    }

    private void Update()
    {
        if (_attackCooldown > 0)
            _attackCooldown -= Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        if (distanceToPlayer <= stats.range)
        {
            _agent.isStopped = true;
            RotateTowardsPlayer();                  // manual rotation when stopped
            TryAttack();
        }
        else
        {
            _agent.isStopped = false;
            _agent.SetDestination(_player.position);

            // manual rotation while moving
            if (_agent.velocity.sqrMagnitude > 0.1f)
                RotateTowardsVelocity();
        }
    }

    private void InitializeStats()
    {
        _agent.speed        = stats.movementSpeed;
        _agent.angularSpeed = stats.turnSpeed;     // degrees per second
        _agent.acceleration = stats.acceleration;  // units per second²

        weapon.Damage       = stats.weaponDamage;
        weapon.Knockback    = stats.weaponKnockback;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.SetColor(BaseColor1, stats.variantColor);

        BaseColor = stats.variantColor;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0;
        Quaternion target = Quaternion.LookRotation(dir);
        float maxDelta = stats.turnSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, maxDelta);
    }

    private void RotateTowardsVelocity()
    {
        Vector3 dir = _agent.velocity;
        dir.y = 0;
        Quaternion target = Quaternion.LookRotation(dir.normalized);
        float maxDelta = stats.turnSpeed * Time.deltaTime;
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
