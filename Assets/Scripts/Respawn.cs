using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class Respawn : MonoBehaviour
{
    [SerializeField] private float dampenFactor = 0.5f;

    private Rigidbody _rb;
    private Transform _spawnPoint;
    private const int respawnCost = 10;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        var spawnObj = GameObject.FindGameObjectWithTag("spawnpoint");
        if (spawnObj != null)
            _spawnPoint = spawnObj.transform;
        else
            Debug.LogWarning("Spawnpoint with tag 'spawnpoint' not found in the scene.");
    }

    private void Update()
    {
        if (transform.position.y <= -15f && _spawnPoint)
        {
            
            // Attempt to charge the player
            if (!CurrencyManager.Instance.SpendMoney(respawnCost))
            {
               
                GameMaster.Instance.TriggerGameOver();
                enabled = false; 
                return;
            }

            // Reset position
            transform.position = _spawnPoint.position;

            // Reset rotation
            transform.rotation = Quaternion.identity;

            // Reset velocity and dampen as needed
            var velocity = _rb.velocity;
            var downward = Mathf.Min(velocity.y, 0);
            velocity.x *= dampenFactor;
            velocity.z *= dampenFactor;
            velocity.y = downward;
            _rb.velocity = velocity;

            // Clear angular velocity
            _rb.angularVelocity = Vector3.zero;

            // Reset accumulated knockback
            var targetComponent = GetComponent<Target>();
            if (targetComponent)
                targetComponent.ResetAccumulatedKnockback();

            // Re-freeze constraints if needed (adjust as appropriate)
            _rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Optionally re-enable any previously disabled components
            var target = GetComponent<Target>();
            if (target && target.CompareTag("Player"))
            {
                GetComponent<PlayerController>().enabled = true;
            }
            else
            {
                var enemy = GetComponent<Enemy>();
                if (enemy)
                {
                    enemy.enabled = true;
                    var agent = GetComponent<NavMeshAgent>();
                    if (agent)
                        agent.enabled = true;
                }
            }

            // Set layer back if it was changed
            gameObject.layer = 0;
        }
    }
}