using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyStats stats;
    [SerializeField] private Weapon weapon;
    
    private Transform _player;
    private NavMeshAgent _agent;
    private float _attackCooldown;

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
        GetComponent<Renderer>().material.color = stats.variantColor;
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