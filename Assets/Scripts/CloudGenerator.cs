using UnityEngine;

[ExecuteInEditMode]
public class CloudGenerator : MonoBehaviour
{
    public int cloudCount = 20;
    public float spawnArea = 50f;
    public Material cloudMaterial;
    public Vector2 windDirection = Vector2.one;
    public float windSpeed = 0.5f;

    private Transform[] _cloudPlanes;

    void Start()
    {
        GenerateClouds();
    }

    void GenerateClouds()
    {
        _cloudPlanes = new Transform[cloudCount];
        
        for(int i = 0; i < cloudCount; i++)
        {
            GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cloud.transform.SetParent(transform);
            cloud.transform.localPosition = new Vector3(
                Random.Range(-spawnArea, spawnArea),
                Random.Range(-spawnArea, spawnArea),
                Random.Range(-spawnArea/2, spawnArea/2)
            );
            
            cloud.GetComponent<Renderer>().material = cloudMaterial;
            _cloudPlanes[i] = cloud.transform;
        }
    }

    void Update()
    {
        cloudMaterial.SetVector("_WindDirection", windDirection.normalized * windSpeed);
        
        foreach(Transform cloud in _cloudPlanes)
        {
            Vector3 newPos = cloud.position + 
                             (Vector3)(windDirection.normalized) * (windSpeed * Time.deltaTime);
            
            if(Vector3.Distance(newPos, transform.position) > spawnArea)
            {
                newPos = transform.position + 
                         Random.insideUnitSphere * spawnArea;
            }
            
            cloud.position = newPos;
        }
    }
}