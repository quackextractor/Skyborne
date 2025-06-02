using ScriptObj;
using UnityEngine;

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
    
    private GameObject _player;
    private Target _targetComponent;
    private bool _isGodMode;
    private bool _isCheating;


    private void Start()
    {
        SpawnEnemies(initialEnemies);
        levelTransitionController.levelLoader.LoadNextLevel();
        
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player != null)
        {
            _targetComponent = _player.GetComponent<Target>();
        }
    }

    private void Update()
    {
        if (!_isCheating)
        {
            return;
        }
        
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
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameMaster.Instance.ClearLevel();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            CurrencyManager.Instance.AddMoney(10);
        }
        
        if (Input.GetKeyDown(KeyCode.G) && _targetComponent != null)
        {
            _isGodMode = !_isGodMode;
            _targetComponent.KnockbackMultiplier = (_isGodMode ? 0 : 1);
            Debug.Log("God Mode: " + (_isGodMode ? "ON" : "OFF"));
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
