using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Enemy/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    public Color variantColor = Color.white;

    [Header("Combat Stats")] 
    public float range          = 1.5f;
    public float attackSpeed    = 1f;
    public float resistance     = 1f;

    [Header("Movement Stats")] 
    public float movementSpeed  = 3f;
    public float turnSpeed      = 720f;   // degrees per second
    public float acceleration   = 20f;    // units per second²

    [Header("Weapon Settings")] 
    public float weaponKnockback = 10f;
    public float weaponDamage    = 5f;
}