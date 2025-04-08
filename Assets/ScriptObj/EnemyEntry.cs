using System.Collections.Generic;
using UnityEngine;

namespace ScriptObj
{
    [System.Serializable]
    public class EnemyEntry
    {
        [Header("Enemy Settings")]
        public Enemy enemyPrefab;              // Reference to the enemy prefab to instantiate.
        public EnemyStats enemyStats;          // Stats to assign to the enemy.
        public Weapon enemyWeapon;             // Changed from EnemyWeapon to Weapon

        [Header("Spawn Position (X,Y)")]
        [Tooltip("x and y positions are limited to your desired interval.")]
        public Vector2 position;
    }

    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Level/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Enemy Spawn Data")]
        public List<EnemyEntry> enemyEntries;
    }
}