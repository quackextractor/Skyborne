using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enemyCountText;
    public GameObject completionMessage;
    public GameObject gameCompletedMessage;

    private void OnEnable()
    {
        // Subscribe to events
        GameMaster.OnEnemyCountChanged += UpdateEnemyCount;
        GameMaster.OnLevelCompleted += ShowLevelCompleted;
        GameMaster.OnGameCompleted += ShowGameCompleted;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        GameMaster.OnEnemyCountChanged -= UpdateEnemyCount;
        GameMaster.OnLevelCompleted -= ShowLevelCompleted;
        GameMaster.OnGameCompleted -= ShowGameCompleted;
    }

    private void Start()
    {
        UpdateLevelText();
        if (completionMessage != null)
            completionMessage.SetActive(false);
        if (gameCompletedMessage != null)
            gameCompletedMessage.SetActive(false);
    }

    private void UpdateEnemyCount(int count)
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Enemies: {count}";
        }
    }

    private void UpdateLevelText()
    {
        if (levelText != null && GameMaster.Instance != null && GameMaster.Instance.levelLoader != null)
        {
            int currentLevel = GameMaster.Instance.levelLoader.GetCurrentLevelIndex() + 1;
            int totalLevels = GameMaster.Instance.levelLoader.GetTotalLevels();
            levelText.text = $"Level {currentLevel} / {totalLevels}";
        }
    }

    private void ShowLevelCompleted()
    {
        if (completionMessage != null)
        {
            completionMessage.SetActive(true);
            // Hide after 3 seconds
            Invoke(nameof(HideCompletionMessage), 3f);
        }
    }

    private void HideCompletionMessage()
    {
        if (completionMessage != null)
            completionMessage.SetActive(false);
    }

    private void ShowGameCompleted()
    {
        if (gameCompletedMessage != null)
        {
            gameCompletedMessage.SetActive(true);
        }
    }
}