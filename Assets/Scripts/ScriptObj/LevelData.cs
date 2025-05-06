using System.Collections.Generic;
using UnityEngine;

namespace ScriptObj
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Level/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Enemy Spawn Data")]
        public List<EnemyEntry> enemyEntries;
    }
}