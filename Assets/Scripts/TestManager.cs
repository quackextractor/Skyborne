using UnityEngine;
using UnityEngine.SceneManagement;

public class TestManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int initialEnemies = 3;
    [SerializeField] private Vector3 spawnArea = new Vector3(5, 0, 5);

    void Start()
    {
        SpawnEnemies(initialEnemies);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnEnemies(1);
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }
    }

    void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                0,
                Random.Range(-spawnArea.z, spawnArea.z)
            );
            Instantiate(enemyPrefab, pos, Quaternion.identity);
        }
    }

    void ResetScene()
    {
        SceneManager.LoadScene(0);
    }
}