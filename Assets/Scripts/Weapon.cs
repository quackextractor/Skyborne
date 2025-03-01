using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float baseKnockback = 10f;
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float attackDuration = 0.5f; // Increased duration
    [SerializeField] private Collider weaponCollider; // Reference to the weapon's collider

    private bool _isAttacking = false;

    private void Start()
    {
        weaponCollider.enabled = false; // Disable collider by default
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        _isAttacking = true;
        weaponCollider.enabled = true; // Enable collider at attack start
        yield return new WaitForSeconds(attackDuration);
        weaponCollider.enabled = false; // Disable collider after attack
        _isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isAttacking) return;
        
        if (other.TryGetComponent<Target>(out Target target))
        {
            Vector3 attackOrigin = transform.position;
            Attack newAttack = new Attack(baseKnockback, baseDamage, attackOrigin);
            target.TakeAttack(newAttack);
        }
    }
}