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

    public void Setup(EnemyStats newStats)
    {
        stats = newStats;
        InitializeStats();
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (_attackCooldown > 0){
            _attackCooldown -= Time.deltaTime;
        }
        var distanceToPlayer = Vector3.Distance(transform.position, _player.position);

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

        var renderer = GetComponent<Renderer>();
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