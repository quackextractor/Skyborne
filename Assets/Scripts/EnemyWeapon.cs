using UnityEngine;

public class EnemyWeapon : Weapon
{
    [Header("Enemy Specific")]
    [SerializeField] private bool canBeParried = true;
    [SerializeField] private bool isRanged = false;

    public void SetDamage(float damage) => this.Damage = damage;
    public void SetKnockback(float knockback) => this.Knockback = knockback;
}