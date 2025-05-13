using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public event Action<int> OnBalanceChanged;
    public event Action<int> OnMoneyGained;
    public event Action<int> OnMoneySpent;

    public int Balance { get; private set; }
    public int TotalGained { get; private set; }
    public int TotalSpent { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoney(int amount)
    {
        Balance += amount;
        TotalGained += amount;
        OnBalanceChanged?.Invoke(Balance);
        OnMoneyGained?.Invoke(amount);
    }

    public bool SpendMoney(int amount)
    {
        if (Balance < amount) return false;
        Balance -= amount;
        TotalSpent += amount;
        OnBalanceChanged?.Invoke(Balance);
        OnMoneySpent?.Invoke(amount);
        return true;
    }
}