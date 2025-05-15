using ScriptObj;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestManager : MonoBehaviour
{
    [Header("Enemy Prefab & Stats")]
    [Tooltip("Drag in your Enemy prefab (the one with the Enemy component).")]
    [SerializeField]
    private Enemy enemyPrefab;

    [Tooltip("Default stats to apply to each spawned enemy.")]
    [SerializeField]
    private EnemyStats defaultStats;

    [Header("Spawn Settings")]
    [SerializeField] private int initialEnemies = 3;
    [SerializeField] private Vector3 spawnArea = new(5, 0, 5);

    [Header("Level Transition")]
    [Tooltip("Reference to the LevelTransitionController in the scene.")]
    [SerializeField] private LevelTransitionController levelTransitionController;

    private void Start()
    {
        SpawnEnemies(initialEnemies);
        levelTransitionController.levelLoader.LoadNextLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Trigger the level transition
            if (levelTransitionController != null)
            {
                levelTransitionController.StartTransition();
            }
            else
            {
                Debug.LogWarning("LevelTransitionController reference is missing on TestManager.");
            }
        }

        if (Input.GetKeyDown(KeyCode.R)){
            ResetScene();
        }

        // Optional: keep spawning on another key if needed
        if (Input.GetKeyDown(KeyCode.X))
        {
            SpawnEnemies(1);
        }
    }

    private void SpawnEnemies(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var pos = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                spawnArea.y,
                Random.Range(-spawnArea.z, spawnArea.z)
            );

            var enemyInstance = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemyInstance.Setup(defaultStats);
        }
    }

    private void ResetScene()
    {
        GameMaster.Instance.ResetLevel();
    }
}
