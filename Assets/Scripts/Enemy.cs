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
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Setup(EnemyStats newStats)
    {
        stats = newStats;
        InitializeStats();

        // After initialization, ensure the enemy's NavMeshAgent is on a valid NavMesh.
        if (_agent != null)
        {
            // Check for a valid nearby NavMesh position within a small radius.
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning("Enemy not near a valid NavMesh area after spawn. Warp failed.");
            }
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
            TryAttack();
        }
        else
        {
            _agent.isStopped = false;
            _agent.SetDestination(_player.position);
        }
    }

    private void InitializeStats()
    {
        _agent.speed = stats.movementSpeed;
        weapon.Damage = stats.weaponDamage;
        weapon.Knockback = stats.weaponKnockback;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.SetColor(BaseColor1, stats.variantColor);
        }
        BaseColor = stats.variantColor;
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
