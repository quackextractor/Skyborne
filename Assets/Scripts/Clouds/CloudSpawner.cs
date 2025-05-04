using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("Prefabs & Pooling")]
    [Tooltip("Cloud prefabs must have a CloudBehavior component.")]
    public GameObject[] cloudPrefabs;
    private Dictionary<GameObject, List<GameObject>> pool = new Dictionary<GameObject, List<GameObject>>();

    [Header("Spawn Volume")]
    public float rangeY = 5f;
    [Tooltip("Distance from center along movement axis where clouds spawn.")]
    public float spawnDistance = 20f;
    [Tooltip("Lateral variance perpendicular to movement axis.")]
    public float lateralRange = 10f;
    public bool isX = true;
    [Tooltip("If true, clouds move in the positive axis direction; otherwise, negative.")]
    public bool reverseDirection = false;

    [Header("Movement")]
    public float minSpeed = 1f, maxSpeed = 3f;

    [Header("Size Modulation")]
    public bool sizeModEnabled = true;
    public float minSize = 0.8f, maxSize = 1.2f;

    [Header("Lifetime")]
    [Tooltip("Minimum cloud lifetime in seconds.")]
    public float minLifetime = 5f;
    [Tooltip("Maximum cloud lifetime in seconds.")]
    public float maxLifetime = 10f;

    [Header("Spawn Timing")]
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cloudPrefabs != null)
        {
            var list = new List<GameObject>(cloudPrefabs);
            int removed = list.RemoveAll(p => p == null);
            if (removed > 0)
            {
                cloudPrefabs = list.ToArray();
                Debug.LogWarning($"CloudSpawner: Removed {removed} null entries from cloudPrefabs.");
            }
        }
        // Ensure min <= max
        minLifetime = Mathf.Min(minLifetime, maxLifetime);
        maxLifetime = Mathf.Max(minLifetime, maxLifetime);
    }
#endif

    private void Start()
    {
        // Validate prefabs
        if (cloudPrefabs == null || cloudPrefabs.Length == 0)
        {
            Debug.LogError("CloudSpawner: No cloudPrefabs assigned.");
            enabled = false;
            return;
        }

        // Initialize pool
        foreach (var prefab in cloudPrefabs)
            pool[prefab] = new List<GameObject>();

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (enabled)
        {
            SpawnCloud();
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    private GameObject GetPooledInstance(GameObject prefab)
    {
        if (prefab == null) return null;

        if (!pool.ContainsKey(prefab))
            pool[prefab] = new List<GameObject>();

        foreach (var go in pool[prefab])
            if (!go.activeInHierarchy)
                return go;

        var newObj = Instantiate(prefab, transform);
        newObj.SetActive(false);
        pool[prefab].Add(newObj);
        return newObj;
    }

    private void SpawnCloud()
    {
        // Choose random prefab
        var prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        if (prefab == null)
        {
            Debug.LogError("CloudSpawner: Encountered null prefab in array.");
            return;
        }

        var cloud = GetPooledInstance(prefab);
        if (cloud == null)
            return;

        // Compute spawn position
        Vector3 centerPos = transform.position;
        Vector3 pos = centerPos;

        // Vertical offset
        pos.y += Random.Range(-rangeY, rangeY);

        // Spawn distance along movement axis
        float spawnSign = reverseDirection ? 1f : -1f;
        if (isX)
        {
            pos.x += spawnSign * spawnDistance;
            pos.z += Random.Range(-lateralRange, lateralRange);
        }
        else
        {
            pos.z += spawnSign * spawnDistance;
            pos.x += Random.Range(-lateralRange, lateralRange);
        }

        cloud.transform.position = pos;

        // Initialize behavior
        var behavior = cloud.GetComponent<CloudBehavior>();
        if (behavior == null)
        {
            Debug.LogError("CloudSpawner: Prefab missing CloudBehavior component.");
            return;
        }

        float speed = Random.Range(minSpeed, maxSpeed);
        float size = sizeModEnabled ? Random.Range(minSize, maxSize) : 1f;
        float lifetime = Random.Range(minLifetime, maxLifetime);
        var dir = isX ? Vector3.right : Vector3.forward;
        behavior.Initialize(dir, speed, size, reverseDirection, lifetime);

        cloud.SetActive(true);
    }
}
