using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float knockback = 10f;
    [SerializeField] private float damage = 5f;
    [SerializeField] private float attackTime = 0.5f;
    [SerializeField] private Collider weaponCollider;

    private bool _isAttacking = false;
    // HashSet to store targets hit during the current attack.
    private HashSet<Target> _hitTargets = new HashSet<Target>();

    private void Start()
    {
        weaponCollider.enabled = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_isAttacking)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        _isAttacking = true;
        // Clear previous attack's hit targets.
        _hitTargets.Clear();

        weaponCollider.enabled = true;
        yield return new WaitForSeconds(attackTime);
        weaponCollider.enabled = false;
        _isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isAttacking) return;

        if (other.TryGetComponent(out Target target))
        {
            // If the target has already been hit during this attack, do nothing.
            if (_hitTargets.Contains(target))
                return;

            // Record this target so it won't be hit again during the same attack.
            _hitTargets.Add(target);
            target.TakeAttack(new Attack(knockback, damage, transform.position));
        }
    }
}