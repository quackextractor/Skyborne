using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Attack Settings")] [SerializeField]
    private float knockback = 10f;

    [SerializeField] private float damage = 5f;
    [SerializeField] private float windUpTime = 0.3f;
    [SerializeField] private float activationTime = 0.2f;
    [SerializeField] private float cooldownTime = 0.5f;
    [SerializeField] private Collider weaponCollider;

    protected Transform _attacker;
    protected HashSet<Target> _hitTargets = new();
    protected bool _isAttacking;
    private HitEffectSpawner _hitEffectSpawner;
    private bool _hasHitEffectSpawner = false;

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

    private void Start()
    {
        weaponCollider.enabled = false;
        _attacker = transform.parent; // weapon is direct child of attacker
        if (TryGetComponent(out EnemyWeapon enemyWeapon))
        {
            _hitEffectSpawner = enemyWeapon.GetComponent<HitEffectSpawner>();
            _hasHitEffectSpawner = true;
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (!_isAttacking) return;

        if (other.TryGetComponent(out Target target))
        {
            if (_hitTargets.Contains(target)) return;
            
            _hitTargets.Add(target);
            target.TakeAttack(new Attack(knockback, damage, _attacker.position));
            if (_hasHitEffectSpawner)
            {
                _hitEffectSpawner.SpawnHitEffect(target.transform.position);
            }
        }
    }

    public virtual void StartAttack()
    {
        if (!_isAttacking) StartCoroutine(Attack());
    }

    protected IEnumerator Attack()
    {
        _isAttacking = true;
        _hitTargets.Clear();
        //  Debug.Log("hit" + _hitTargets);
        yield return new WaitForSeconds(windUpTime);

        weaponCollider.enabled = true;
        yield return new WaitForSeconds(activationTime);
        weaponCollider.enabled = false;
        yield return new WaitForSeconds(cooldownTime);
        _isAttacking = false;
    }

    public virtual void AttackEffect()
    {
    }
}