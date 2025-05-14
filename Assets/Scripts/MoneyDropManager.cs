using UnityEngine;

public class MoneyDropManager : MonoBehaviour
{
    public static MoneyDropManager Instance { get; private set; }

    [Tooltip("Prefab of the money pickup (must have MoneyPickup component)")]
    public GameObject moneyPrefab;

    [Tooltip("How many individual coins to spawn per unit of reward")]
    public int coinsPerCash = 1;

    [Tooltip("Spread radius for spawned coins")]
    public float spawnSpread = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
      //  DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Call this when an enemy dies, passing its world position and reward amount.
    /// </summary>
    public void SpawnCoins(Vector3 worldPos, int rewardAmount)
    {
        int totalCoins = rewardAmount * coinsPerCash;
        for (int i = 0; i < totalCoins; i++)
        {
            // random offset in a circle
            Vector2 rand = Random.insideUnitCircle * spawnSpread;
            Vector3 spawnPos = worldPos + new Vector3(rand.x, 0.5f, rand.y);

            GameObject go = Instantiate(moneyPrefab, spawnPos, Quaternion.identity);
            var pickup = go.GetComponent<MoneyPickup>();
            pickup.Initialize(1/coinsPerCash);
        }
    }
}