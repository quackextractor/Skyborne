using ScriptObj;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour
{
    [Header("Level Data")] 
    [Tooltip("List of LevelData ScriptableObjects for each level.")]
    public List<LevelData> levelDataList = new List<LevelData>();

    [Tooltip("Current level index to load (0-based).")]
    public int currentLevelIndex = 0;

    [Header("Spawn Settings")] 
    [Tooltip("Parent object for all spawned enemies (optional).")]
    public Transform enemyParent;

    [Tooltip("The fixed spawn height (Y coordinate) for enemy spawns.")]
    public float spawnHeight;

    [Tooltip("Maximum search radius for a valid NavMesh position.")]
    public float navMeshSampleDistance = 5f;

    [Tooltip("Fallback spawn position (must be on the NavMesh) if no valid position is found.")]
    public Vector3 fallbackSpawnPosition = Vector3.zero;

    [Tooltip("Defines the area for randomly spawning enemies if no position is specified.")]
    public Vector2 spawnArea = new(10f, 10f);

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }

    public int GetTotalLevels()
    {
        return levelDataList.Count;
    }

    public bool IsLastLevel()
    {
        return currentLevelIndex >= levelDataList.Count - 1;
    }

    public void LoadLevel()
    {
        if (levelDataList == null || levelDataList.Count == 0)
        {
            Debug.LogError("No LevelData assigned to LevelLoader!");
            return;
        }

        if (currentLevelIndex < 0 || currentLevelIndex >= levelDataList.Count)
        {
            Debug.LogError($"Invalid level index: {currentLevelIndex}. Valid range is 0 to {levelDataList.Count - 1}");
            return;
        }

        LoadLevelAtIndex(currentLevelIndex);
    }

    public void LoadLevelAtIndex(int index)
    {
        if (index < 0 || index >= levelDataList.Count)
        {
            Debug.LogError($"Invalid level index: {index}. Valid range is 0 to {levelDataList.Count - 1}");
            return;
        }

        currentLevelIndex = index;
        LevelData levelData = levelDataList[currentLevelIndex];

        if (levelData == null)
        {
            Debug.LogError($"LevelData at index {currentLevelIndex} is null!");
            return;
        }

        // Clear existing enemies before spawning new ones
        // ClearExistingEnemies();

        Debug.Log($"Loading level {currentLevelIndex + 1} of {levelDataList.Count}");

        int spawnedEnemies = 0;
        foreach (var entry in levelData.enemyEntries)
        {
            // If the entry has a non-zero position (interpreted as (x, z) data), use it.
            // Otherwise, choose a random position within the defined spawn area.
            Vector3 spawnPosition;
            if (entry.position != Vector2.zero)
                spawnPosition = new Vector3(entry.position.x, spawnHeight, entry.position.y);
            else
                spawnPosition = new Vector3(
                    Random.Range(-spawnArea.x, spawnArea.x),
                    spawnHeight,
                    Random.Range(-spawnArea.y, spawnArea.y)
                );

            // Attempt to sample a nearby valid NavMesh position.
            if (NavMesh.SamplePosition(spawnPosition, out var hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }
            else
            {
                Debug.LogWarning("No valid NavMesh position found near " + spawnPosition.ToString("F2") +
                                 ". Using fallback position.");
                // Use the fallback spawn position.
                var fallbackPos = new Vector3(fallbackSpawnPosition.x, spawnHeight, fallbackSpawnPosition.z);
                if (NavMesh.SamplePosition(fallbackPos, out var fallbackHit, navMeshSampleDistance, NavMesh.AllAreas))
                {
                    spawnPosition = fallbackHit.position;
                }
                else
                {
                    Debug.LogError("No valid NavMesh position found at fallback (" + fallbackPos.ToString("F2") +
                                   "). Skipping enemy spawn.");
                    continue;
                }
            }

            // Instantiate the enemy prefab at the verified spawn position.
            var enemyInstance = Instantiate(entry.enemyPrefab, spawnPosition, Quaternion.identity, enemyParent);
            enemyInstance.Setup(entry.enemyStats);
            spawnedEnemies++;
        }

        Debug.Log($"Spawned {spawnedEnemies} enemies for level {currentLevelIndex + 1}");
    }

    public void LoadNextLevel()
    {
        if (IsLastLevel())
        {
            Debug.Log("Already at the last level!");
            return;
        }

        currentLevelIndex++;
        LoadLevel();
    }

    private void ClearExistingEnemies()
    {
        if (enemyParent != null)
        {
            foreach (Transform child in enemyParent)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            // If no enemy parent is set, find and destroy all enemies in the scene
            Enemy[] existingEnemies = FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in existingEnemies)
            {
                Destroy(enemy.gameObject);
            }
        }
    }
}