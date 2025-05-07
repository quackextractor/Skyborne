using System;
using UnityEngine;

namespace ScriptObj
{
    [Serializable]
    public class EnemyEntry
    {
        [Header("Enemy Settings")] public Enemy enemyPrefab; // Reference to the enemy prefab.

        public EnemyStats enemyStats; // Stats to assign to the enemy.

        [Header("Spawn Position (X,Y)")] [Tooltip("x and y positions are limited to your desired interval.")]
        public Vector2 position;
    }
}