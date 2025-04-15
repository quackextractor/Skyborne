using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class Respawn : MonoBehaviour
{
    private Transform _spawnPoint;
    private Rigidbody _rb; 
    [SerializeField]
    private float dampenFactor = 0.5f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        var spawnObj = GameObject.FindGameObjectWithTag("spawnpoint");
        if (spawnObj != null)
        {
            _spawnPoint = spawnObj.transform;
        }
        else
        {
            Debug.LogWarning("Spawnpoint with tag 'spawnpoint' not found in the scene.");
        }
    }

    private void Update()
    {
        if (transform.position.y <= -15f && _spawnPoint)
        {
            transform.position = _spawnPoint.position;

            var velocity = _rb.velocity;
            // Preserve downward velocity, dampen others
            var downward = Mathf.Min(velocity.y, 0);
            velocity.x *= dampenFactor;
            velocity.z *= dampenFactor;
            velocity.y = downward;

            _rb.velocity = velocity;
        }
    }
}