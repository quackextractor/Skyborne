using UnityEngine;
using UnityEngine.SceneManagement;
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
    public static event Action       OnGameOver;

    // NEW: fired after every level scene finishes loading
    public static event Action       OnLevelLoaded;

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
      //  DontDestroyOnLoad(gameObject);

        // Hook into Unity's sceneLoaded event
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid leaks
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset enemy tracking for the fresh level
        Initialize();

        // Notify listeners that a new level is active
        OnLevelLoaded?.Invoke();
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

    private void HandleEnemySpawned()
    {
        enemyCount++;
        UpdateEnemyCount();
        Debug.Log($"Enemy spawned. Total enemies: {enemyCount}");
    }

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
        yield return new WaitForSeconds(2f);

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
    
    public void TriggerGameOver()
    {
        Debug.Log("Game Over – player couldn't afford respawn.");
        OnGameOver?.Invoke();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}