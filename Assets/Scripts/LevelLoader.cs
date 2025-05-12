using ScriptObj;
using UnityEngine;
using UnityEngine.AI;

public class LevelLoader : MonoBehaviour
{
    [Header("Level Data")] [Tooltip("Assign your LevelData ScriptableObject here.")]
    public LevelData levelData;

    [Header("Spawn Settings")] [Tooltip("Parent object for all spawned enemies (optional).")]
    public Transform enemyParent;

    [Tooltip("The fixed spawn height (Y coordinate) for enemy spawns.")]
    public float spawnHeight;

    [Tooltip("Maximum search radius for a valid NavMesh position.")]
    public float navMeshSampleDistance = 5f;

    [Tooltip("Fallback spawn position (must be on the NavMesh) if no valid position is found.")]
    public Vector3 fallbackSpawnPosition = Vector3.zero;

    [Tooltip("Defines the area for randomly spawning enemies if no position is specified.")]
    public Vector2 spawnArea = new(10f, 10f);

    private void Start()
    {
        LoadLevel();
    }

    public void LoadLevel()
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData not assigned to LevelLoader!");
            return;
        }

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
        }
    }
}