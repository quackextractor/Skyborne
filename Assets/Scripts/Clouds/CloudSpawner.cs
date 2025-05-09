using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clouds
{
    public class CloudSpawner : MonoBehaviour
    {
        [Header("Prefabs & Pooling")] [Tooltip("Cloud prefabs must have a CloudBehavior component.")]
        public GameObject[] cloudPrefabs;

        [Header("Planes")] [Tooltip("Plane GameObject (must have MeshFilter + MeshCollider). Spawn surface.")]
        public MeshCollider startPlane;

        [Tooltip("Plane GameObject (must have MeshFilter + MeshCollider). Target surface.")]
        public MeshCollider endPlane;

        [Header("Timing")] [Tooltip("Time for a cloud to travel from start to end.")]
        public float travelTime = 5f;

        [Header("Size Modulation")] public bool sizeModEnabled = true;
        public float minSize = 0.8f, maxSize = 1.2f;

        [Header("Rotation")] [Tooltip("Randomize cloud rotation on these axes.")]
        public bool randomRotationX;
        public bool randomRotationY = true;
        public bool randomRotationZ;

        [Header("Spawn Timing")] public float minSpawnInterval = 1f;
        public float maxSpawnInterval = 3f;

        [Header("Spawn Control")] [Tooltip("Enable or disable spawning of new clouds.")]
        [SerializeField]
        private bool enableSpawning = true;

        private readonly Dictionary<GameObject, List<GameObject>> pool = new();

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
                if (enableSpawning)
                {
                    SpawnCloud();
                }
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
            if (!prefab) return;

            var cloud = GetPooledInstance(prefab);
            if (!cloud) return;

            // Determine start and end
            var startPos = RandomPointOnPlane(startPlane);
            var endPos = RandomPointOnPlane(endPlane);
            var dir = (endPos - startPos).normalized;
            var distance = Vector3.Distance(startPos, endPos);
            var speed = distance / travelTime;

            // Position
            cloud.transform.position = startPos;

            // Random rotation
            var euler = Vector3.zero;
            if (randomRotationX) euler.x = Random.Range(0f, 360f);
            if (randomRotationY) euler.y = Random.Range(0f, 360f);
            if (randomRotationZ) euler.z = Random.Range(0f, 360f);
            cloud.transform.rotation = Quaternion.Euler(euler);

            // Initialize behavior
            var behavior = cloud.GetComponent<CloudBehavior>();
            if (!behavior)
            {
                Debug.LogError("CloudSpawner: Prefab missing CloudBehavior component.");
                return;
            }

            var size = sizeModEnabled ? Random.Range(minSize, maxSize) : 1f;
            behavior.Initialize(dir, speed, size, travelTime, endPos);
            cloud.SetActive(true);
        }

        private Vector3 RandomPointOnPlane(MeshCollider plane)
        {
            var mf = plane.GetComponent<MeshFilter>();
            var mesh = mf.sharedMesh;
            var trs = plane.transform;

            var localCenter = mesh.bounds.center;
            var localExtents = mesh.bounds.extents;

            var randX = Random.Range(-localExtents.x, localExtents.x);
            var randZ = Random.Range(-localExtents.z, localExtents.z);

            var localPoint = new Vector3(
                localCenter.x + randX,
                localCenter.y,
                localCenter.z + randZ
            );

            return trs.TransformPoint(localPoint);
        }

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
            var trs = plane.transform;
            Gizmos.matrix = Matrix4x4.TRS(trs.position, trs.rotation, trs.lossyScale);
            Gizmos.DrawWireMesh(mesh);
        }
    #endif
    }
}