using UnityEngine;

public class EnemyWeapon : Weapon
{
    [Header("Enemy Specific")] [SerializeField]
    private bool canBeParried = true;

    [SerializeField] private bool isRanged;

    public void SetDamage(float damage)
    {
        Damage = damage;
    }

    public void SetKnockback(float knockback)
    {
        Knockback = knockback;
    }
}