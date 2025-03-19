using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;
    [SerializeField] private Weapon weapon;
    
    private Transform _player;
    private NavMeshAgent _agent;
    private float _attackCooldown;

    public Color BaseColor { get; private set; } // Expose the base color

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        InitializeStats();
    }

    private void InitializeStats()
    {
        _agent.speed = stats.movementSpeed;
        weapon.Damage = stats.weaponDamage;
        weapon.Knockback = stats.weaponKnockback;
        
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.SetColor("_BaseColor", stats.variantColor); // Set shader's base color
        BaseColor = stats.variantColor; // Store the initial color
    }

    private void Update()
    {
        if (_attackCooldown > 0) _attackCooldown -= Time.deltaTime;
        
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

    private void TryAttack()
    {
        if (_attackCooldown <= 0)
        {
            weapon.StartAttack();
            _attackCooldown = 1f / stats.attackSpeed;
        }
    }
}