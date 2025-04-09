using UnityEngine;
using UnityEngine.SceneManagement;

public class TestManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int initialEnemies = 3;
    [SerializeField] private Vector3 spawnArea = new(5, 0, 5);

    private void Start()
    {
        SpawnEnemies(initialEnemies);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) SpawnEnemies(1);

        if (Input.GetKeyDown(KeyCode.R)) ResetScene();
    }

    private void SpawnEnemies(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var pos = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                0,
                Random.Range(-spawnArea.z, spawnArea.z)
            );
            Instantiate(enemyPrefab, pos, Quaternion.identity);
        }
    }

    private void ResetScene()
    {
        SceneManager.LoadScene(1);
    }
}