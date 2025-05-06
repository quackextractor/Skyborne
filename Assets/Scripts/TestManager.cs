using UnityEngine;
using UnityEngine.SceneManagement;

public class TestManager : MonoBehaviour
{
    [Header("Enemy Prefab & Stats")]
    [Tooltip("Drag in your Enemy prefab (the one with the Enemy component).")]
    [SerializeField] private Enemy enemyPrefab;  
    
    [Tooltip("Default stats to apply to each spawned enemy.")]
    [SerializeField] private EnemyStats defaultStats;  

    [Header("Spawn Settings")]
    [SerializeField] private int initialEnemies = 3;
    [SerializeField] private Vector3 spawnArea = new(5, 0, 5);

    private void Start()
    {
        SpawnEnemies(initialEnemies);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            SpawnEnemies(1);

        if (Input.GetKeyDown(KeyCode.R))
            ResetScene();
    }

    private void SpawnEnemies(int count)
    {
        for (var i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                spawnArea.y,
                Random.Range(-spawnArea.z, spawnArea.z)
            );

            // Instantiate as an Enemy directly
            Enemy enemyInstance = Instantiate(enemyPrefab, pos, Quaternion.identity);
            
            // Initialize its stats
            enemyInstance.Setup(defaultStats);
        }
    }

    private void ResetScene()
    {
        // Reload the current scene (index 1)
        SceneManager.LoadScene(1);
    }
}