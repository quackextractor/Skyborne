using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CloudSpawner : MonoBehaviour
{
    [Header("Prefabs & Pooling")]
    [Tooltip("Cloud prefabs must have a CloudBehavior component.")]
    public GameObject[] cloudPrefabs;
    private Dictionary<GameObject, List<GameObject>> pool = new Dictionary<GameObject, List<GameObject>>();

    [Header("Planes")]
    [Tooltip("Plane GameObject (must have MeshFilter + MeshCollider). Spawn surface.")]
    public MeshCollider startPlane;
    [Tooltip("Plane GameObject (must have MeshFilter + MeshCollider). Target surface.")]
    public MeshCollider endPlane;

    [Header("Timing")]
    [Tooltip("Time for a cloud to travel from start to end.")]
    public float travelTime = 5f;

    [Header("Size Modulation")]
    public bool sizeModEnabled = true;
    public float minSize = 0.8f, maxSize = 1.2f;

    [Header("Spawn Timing")]
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        DrawPlaneGizmo(startPlane, Color.green);
        DrawPlaneGizmo(endPlane, Color.red);
    }

    private void DrawPlaneGizmo(MeshCollider plane, Color color)
    {
        if (plane == null) return;
        var mf = plane.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;
        Gizmos.color = color;
        var mesh = mf.sharedMesh;
        var trs  = plane.transform;
        Gizmos.matrix = Matrix4x4.TRS(trs.position, trs.rotation, trs.lossyScale);
        Gizmos.DrawWireMesh(mesh);
    }
#endif

    private void Start()
    {
        if (cloudPrefabs == null || cloudPrefabs.Length == 0)
        {
            Debug.LogError("CloudSpawner: No cloudPrefabs assigned.");
            enabled = false;
            return;
        }
        if (startPlane == null || endPlane == null)
        {
            Debug.LogError("CloudSpawner: StartPlane and EndPlane must be assigned.");
            enabled = false;
            return;
        }
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
        var prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        if (prefab == null) return;

        var cloud = GetPooledInstance(prefab);
        if (cloud == null) return;

        Vector3 startPos = RandomPointOnPlane(startPlane);
        Vector3 endPos   = RandomPointOnPlane(endPlane);
        Vector3 dir      = (endPos - startPos).normalized;
        float distance   = Vector3.Distance(startPos, endPos);
        float speed      = distance / travelTime;

        cloud.transform.position = startPos;
        var behavior = cloud.GetComponent<CloudBehavior>();
        if (behavior == null)
        {
            Debug.LogError("CloudSpawner: Prefab missing CloudBehavior component.");
            return;
        }

        float size = sizeModEnabled ? Random.Range(minSize, maxSize) : 1f;
        behavior.Initialize(dir, speed, size, travelTime, endPos);
        cloud.SetActive(true);
    }

    private Vector3 RandomPointOnPlane(MeshCollider plane)
    {
        // Use MeshFilter bounds (local) and then TransformPoint to apply scale/rotation
        var mf   = plane.GetComponent<MeshFilter>();
        var mesh = mf.sharedMesh;
        var trs  = plane.transform;

        Vector3 localCenter  = mesh.bounds.center;
        Vector3 localExtents = mesh.bounds.extents;

        float randX = Random.Range(-localExtents.x, localExtents.x);
        float randZ = Random.Range(-localExtents.z, localExtents.z);

        Vector3 localPoint = new Vector3(
            localCenter.x + randX,
            localCenter.y,
            localCenter.z + randZ
        );

        return trs.TransformPoint(localPoint);
    }
}

