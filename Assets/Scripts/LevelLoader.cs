using ScriptObj;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [Header("Level Data")]
    [Tooltip("Assign your LevelData ScriptableObject here.")]
    public LevelData levelData;

    [Header("Spawn Settings")]
    [Tooltip("Parent object for all spawned enemies (optional).")]
    public Transform enemyParent;

    [Tooltip("The fixed z coordinate for enemy spawns.")]
    public float fixedZ = 0f;

    void Start()
    {
        LoadLevel();
    }

    void LoadLevel()
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData not assigned to LevelLoader!");
            return;
        }

        foreach (var entry in levelData.enemyEntries)
        {
            // Construct a spawn position using the provided x, y and a fixed z.
            Vector3 spawnPosition = new Vector3(entry.position.x, entry.position.y, fixedZ);

            // Instantiate the enemy prefab.
            Enemy enemyInstance = Instantiate(entry.enemyPrefab, spawnPosition, Quaternion.identity, enemyParent);

            // IMPORTANT: Use a Setup method on your Enemy script to assign stats and weapon.
            // This method should override the defaults that are set in Awake.
            enemyInstance.Setup(entry.enemyStats);
        }
    }
}