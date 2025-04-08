using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Enemy/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    public enum EnemyType
    {
        Warmonger,
        Brute,
        Fighter,
        Archer
    }

    [Header("Type")] public EnemyType enemyType;

    public Color variantColor = Color.white;

    [Header("Combat Stats")] public float range = 1.5f;

    public float attackSpeed = 1f;
    public float resistance = 1f;
    public float movementSpeed = 3f;

    [Header("Weapon Settings")] public float weaponKnockback = 10f;

    public float weaponDamage = 5f;
}