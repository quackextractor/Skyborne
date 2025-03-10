using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float knockback = 10f;
    [SerializeField] private float damage = 5f;
    [SerializeField] private float windUpTime = 0.3f;
    [SerializeField] private float activationTime = 0.2f;
    [SerializeField] private float cooldownTime = 0.5f;
    [SerializeField] private Collider weaponCollider;

    public float Knockback
    {
        get => knockback;
        set => knockback = value;
    }

    public float Damage
    {
        get => damage;
        set => damage = value;
    }

    private Transform _attacker;
    private bool _isAttacking = false;
    private HashSet<Target> _hitTargets = new HashSet<Target>();

    private void Start()
    {
        weaponCollider.enabled = false;
        _attacker = transform.parent; // Assuming weapon is direct child of attacker
    }

    public void StartAttack()
    {
        if (!_isAttacking)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        _isAttacking = true;
        _hitTargets.Clear();

        yield return new WaitForSeconds(windUpTime);
        
        weaponCollider.enabled = true;
        yield return new WaitForSeconds(activationTime);
        weaponCollider.enabled = false;
        yield return new WaitForSeconds(cooldownTime);
        _isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isAttacking) return;

        if (other.TryGetComponent(out Target target))
        {
            if (_hitTargets.Contains(target)) return;
            
            _hitTargets.Add(target);
            target.TakeAttack(new Attack(knockback, damage, _attacker.position));
        }
    }
}