using UnityEngine;
using UnityEngine.Serialization;

namespace Abilities
{
    public class Fly : MonoBehaviour
    {
        [FormerlySerializedAs("_timestamp")] [SerializeField] private float timestamp = 4;
        [FormerlySerializedAs("_attack")] [SerializeField] private float attack = 10;
        [FormerlySerializedAs("_knockback")] [SerializeField] private float knockback = 30;
        [SerializeField] private float speed = 0.1f;
        private Vector3 _position;

        private GameObject player;

        // Start is called before the first frame update
        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            _position = player.transform.forward.normalized;
            transform.position = player.transform.position + player.transform.forward;
            timestamp += Time.time;
        }

        // Update is called once per frame
        private void Update()
        {
            if (timestamp < Time.time) Destroy(gameObject);
            transform.position += _position * (speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.name != "Player" && other.TryGetComponent<Target>(out var target))
            {
                target.TakeAttack(_position, attack, knockback);
                // target.ApplyKnockbackForce(_position, _knockback*2);
                Destroy(gameObject);
            }
        }
    }
}