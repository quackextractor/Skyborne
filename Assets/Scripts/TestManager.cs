using ScriptObj;
using UnityEngine;
using System.Collections.Generic;

public class TestManager : MonoBehaviour
{
    [Header("Enemy Prefab & Stats")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private EnemyStats defaultStats;

    [Header("Spawn Settings")]
    [SerializeField] private int initialEnemies = 3;
    [SerializeField] private Vector3 spawnArea = new(5, 0, 5);

    [Header("Level Transition")]
    [SerializeField] private LevelTransitionController levelTransitionController;

    [Header("Cheat Settings")]
    [Tooltip("UI element or indicator to show cheats are enabled.")]
    [SerializeField] private GameObject cheatIndicator;

    private GameObject _player;
    private Target _targetComponent;
    private bool _isGodMode;
    private bool _isCheating;

    private readonly Queue<char> _inputBuffer = new();
    private const string CheatCode = "mvh";
    private const int MaxBufferSize = 10;
    private const string CheatPrefKey = "IsCheating";

    private void Start()
    {
        _isCheating = PlayerPrefs.GetInt(CheatPrefKey, 0) == 1;
        UpdateCheatIndicator();

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
        HandleCheatToggleInput();

        if (!_isCheating) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            levelTransitionController?.StartTransition();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }

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

    private void HandleCheatToggleInput()
    {
        foreach (char c in Input.inputString.ToLower())
        {
            if (!char.IsLetter(c)) continue;

            _inputBuffer.Enqueue(c);
            if (_inputBuffer.Count > MaxBufferSize)
                _inputBuffer.Dequeue();

            string currentInput = new string(_inputBuffer.ToArray());

            if (currentInput.EndsWith(CheatCode))
            {
                _isCheating = !_isCheating;
                PlayerPrefs.SetInt(CheatPrefKey, _isCheating ? 1 : 0);
                PlayerPrefs.Save();

                Debug.Log("Cheats " + (_isCheating ? "ENABLED" : "DISABLED"));
                UpdateCheatIndicator();
                _inputBuffer.Clear();
            }
        }
    }

    private void UpdateCheatIndicator()
    {
        if (cheatIndicator != null)
        {
            cheatIndicator.SetActive(_isCheating);
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
