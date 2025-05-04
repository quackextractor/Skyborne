using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("Prefabs & Pooling")]
    [Tooltip("Cloud prefabs must have a Renderer and CloudBehavior component attached.")]
    public GameObject[] cloudPrefabs;
    private List<GameObject> pool = new List<GameObject>();

    [Header("Spawn Volume")]
    [Tooltip("Range around this GameObject's Y position: spawns between centerY - rangeY and centerY + rangeY")]  
    public float rangeY = 5f;
    [Tooltip("Range around this GameObject's horizontal axis: spawns between centerH - rangeH and centerH + rangeH")]  
    public float rangeH = 10f;
    public bool isX = true;

    [Header("Movement & Fade")]
    public float fadeDistance = 20f;
    public float minSpeed = 1f, maxSpeed = 3f;

    [Header("Size Modulation")]
    public bool sizeModEnabled = true;
    public float minSize = 0.8f, maxSize = 1.2f;

    [Header("Spawn Timing")]
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnCloud();
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    private GameObject GetPooledInstance()
    {
        // Reuse inactive
        foreach (var go in pool)
        {
            if (!go.activeInHierarchy)
                return go;
        }
        // Instantiate new
        if (cloudPrefabs == null || cloudPrefabs.Length == 0)
        {
            Debug.LogError("CloudSpawner: No cloudPrefabs assigned.");
            return null;
        }
        GameObject prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        GameObject newObj = Instantiate(prefab, transform);
        pool.Add(newObj);
        return newObj;
    }

    private void SpawnCloud()
    {
        Vector3 center = transform.position;

        // Determine spawn offsets
        float yOffset = Random.Range(-rangeY, rangeY);
        float hOffset = Random.Range(-rangeH, rangeH);

        // Calculate position
        Vector3 pos = center;
        pos.y += yOffset;
        if (isX)
            pos.x += (hOffset > 0 ? fadeDistance : -fadeDistance) + hOffset;
        else
            pos.z += (hOffset > 0 ? fadeDistance : -fadeDistance) + hOffset;

        // Get or create cloud
        GameObject cloud = GetPooledInstance();
        if (cloud == null)
            return;

        cloud.transform.position = pos;

        // Initialize behavior
        CloudBehavior behavior = cloud.GetComponent<CloudBehavior>();
        if (behavior == null)
        {
            Debug.LogError("CloudSpawner: Cloud prefab missing CloudBehavior component.");
            return;
        }
        behavior.Initialize(
            center,
            isX ? Vector3.right : Vector3.forward,
            Random.Range(minSpeed, maxSpeed),
            fadeDistance,
            sizeModEnabled ? Random.Range(minSize, maxSize) : 1f
        );

        cloud.SetActive(true);
    }
}