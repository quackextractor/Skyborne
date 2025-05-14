using System;
using System.Collections;
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
    public TextMeshProUGUI balanceText;
    public Color gainColor = Color.green;
    public Color spendColor = Color.red;
    public float flashDuration = 0.5f;

    private CurrencyManager _currency;

    private void OnEnable()
    {
        GameMaster.OnEnemyCountChanged  += UpdateEnemyCount;
        GameMaster.OnLevelCompleted     += ShowLevelCompleted;
        GameMaster.OnGameCompleted      += ShowGameCompleted;
        GameMaster.OnLevelLoaded        += UpdateLevelText;

        _currency = CurrencyManager.Instance;
        if (_currency != null)
        {
            _currency.OnBalanceChanged += UpdateBalance;
            _currency.OnMoneyGained   += FlashGain;
            _currency.OnMoneySpent    += FlashSpend;
        }
    }

    private void OnDisable()
    {
        GameMaster.OnEnemyCountChanged  -= UpdateEnemyCount;
        GameMaster.OnLevelCompleted     -= ShowLevelCompleted;
        GameMaster.OnGameCompleted      -= ShowGameCompleted;
        GameMaster.OnLevelLoaded        -= UpdateLevelText;

        if (_currency != null)
        {
            _currency.OnBalanceChanged  -= UpdateBalance;
            _currency.OnMoneyGained     -= FlashGain;
            _currency.OnMoneySpent      -= FlashSpend;
        }
    }

    private void Start()
    {
        // initial draw
        UpdateLevelText();
        if (completionMessage != null)
            completionMessage.SetActive(false);
        if (gameCompletedMessage != null)
            gameCompletedMessage.SetActive(false);
    }

    private void UpdateBalance(int newBal)
    {
        if (balanceText != null)
            balanceText.text = $"${newBal}";
    }

    private void FlashGain(int amount) => StartCoroutine(FlashColor(gainColor));
    private void FlashSpend(int amount) => StartCoroutine(FlashColor(spendColor));

    private IEnumerator FlashColor(Color c)
    {
        if (balanceText == null)
            yield break;

        var orig = balanceText.color;
        balanceText.color = c;
        yield return new WaitForSeconds(flashDuration);
        balanceText.color = orig;
    }

    private void UpdateEnemyCount(int count)
    {
        if (enemyCountText != null)
            enemyCountText.text = $"Enemies: {count}";
    }

    private void UpdateLevelText()
    {
        if (levelText != null && GameMaster.Instance?.levelLoader != null)
        {
            int currentLevel = GameMaster.Instance.levelLoader.GetCurrentLevelIndex() + 1;
            int totalLevels  = GameMaster.Instance.levelLoader.GetTotalLevels();
            levelText.text   = $"Level {currentLevel} / {totalLevels}";
        }
    }

    private void ShowLevelCompleted()
    {
        if (completionMessage != null)
        {
            completionMessage.SetActive(true);
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
            gameCompletedMessage.SetActive(true);
    }

    private void Update()
    {
        UpdateLevelText();
    }
}
