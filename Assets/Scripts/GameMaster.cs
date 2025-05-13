using UnityEngine;
using System;
using System.Collections;

public class GameMaster : MonoBehaviour
{   
    [Header("Enemy Tracking")]
    [SerializeField]
    private int enemyCount = 0;

    [Header("References")]
    public LevelLoader levelLoader;
    public LevelTransitionController levelTransitionController;
    
    public static event Action<int> OnEnemyCountChanged;
    public static event Action       OnLevelCompleted;
    public static event Action       OnGameCompleted;

    private static GameMaster _instance;
    public static GameMaster Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameMaster>();
                if (_instance == null)
                {
                    var go = new GameObject("GameMaster");
                    _instance = go.AddComponent<GameMaster>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Enemy.OnEnemySpawned += HandleEnemySpawned;
    }

    private void OnDisable()
    {
        Enemy.OnEnemySpawned -= HandleEnemySpawned;
    }

    public void Initialize()
    {
        enemyCount = 0;
        UpdateEnemyCount();
    }

    // This is your callback from Enemy.OnEnemySpawned
    private void HandleEnemySpawned()
    {
        enemyCount++;
        UpdateEnemyCount();
        Debug.Log($"Enemy spawned. Total enemies: {enemyCount}");
    }

    // You’ll need to invoke this from wherever you detect an enemy’s death:
    public void OnEnemyDefeated()
    {
        enemyCount--;
        UpdateEnemyCount();
        Debug.Log($"Enemy defeated. Remaining enemies: {enemyCount}");

        if (enemyCount <= 0)
            HandleLevelCompletion();
    }

    private void UpdateEnemyCount()
    {
        OnEnemyCountChanged?.Invoke(enemyCount);
    }

    private void HandleLevelCompletion()
    {
        Debug.Log("Level completed!");
        OnLevelCompleted?.Invoke();

        if (levelLoader.IsLastLevel())
        {
            Debug.Log("Game completed!");
            OnGameCompleted?.Invoke();
        }
        else
        {
            StartCoroutine(StartLevelTransition());
        }
    }

    private IEnumerator StartLevelTransition()
    {
        yield return new WaitForSeconds(2f);    // small pause

        if (levelTransitionController != null)
            levelTransitionController.StartTransition();
    }

    public int GetEnemyCount()
    {
        return enemyCount;
    }

    public void ResetLevel()
    {
        Initialize();
        levelLoader.LoadLevel();
    }
}
