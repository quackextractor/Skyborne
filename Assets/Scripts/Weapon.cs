using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Weapon : MonoBehaviour
{
    [Header("Attack Settings")] [SerializeField]
    private float knockback = 10f;

    [SerializeField] private float damage = 5f;
    [SerializeField] private float windUpTime = 0.3f;
    [SerializeField] private float activationTime = 0.2f;
    [SerializeField] private float cooldownTime = 0.5f;
    [SerializeField] protected Collider weaponCollider;
    [FormerlySerializedAs("_isAttacking")] public bool isAttacking;
    protected readonly HashSet<Target> _hitTargets = new();

    private Transform _attacker;
    private bool _hasHitEffectSpawner;
    private HitEffectSpawner _hitEffectSpawner;

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
    public float WindUpTime { get => windUpTime; set => windUpTime = value; }
    public float ActivationTime { get => activationTime; set => activationTime = value; }
    public float CooldownTime { get => cooldownTime; set => cooldownTime = value; }

    private void Start()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
        _attacker = transform.parent;

        if (TryGetComponent(out _hitEffectSpawner)) _hasHitEffectSpawner = true;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (!isAttacking) return;

        if (other.TryGetComponent(out Target target))
        {
            if (!_hitTargets.Add(target)) return;

            target.TakeAttack(new Attack(knockback, damage, _attacker.position));
            if (_hasHitEffectSpawner) _hitEffectSpawner.SpawnHitEffect(target.transform.position);
        }
    }

    public virtual void StartAttack()
    {
        if (!isAttacking) StartCoroutine(Attack());
    }

    protected IEnumerator Attack()
    {
        isAttacking = true;
        _hitTargets.Clear();
        //  Debug.Log("hit" + _hitTargets);
        yield return new WaitForSeconds(WindUpTime);
        weaponCollider.enabled = true;
        yield return new WaitForSeconds(ActivationTime);
        weaponCollider.enabled = false;
        yield return new WaitForSeconds(CooldownTime);
  
        isAttacking = false;
    }

    public virtual void AttackEffect()
    {
       
    }
}