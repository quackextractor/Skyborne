// --- Fly.cs ---
using UnityEngine;

namespace Abilities
{
    public class Fly : MonoBehaviour
    {
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private float attack = 10f;
        [SerializeField] private float knockback = 30f;
        [SerializeField] private float speed = 0.1f;

        [Header("VFX & SFX")]
        [SerializeField] private ParticleSystem enemyFireParticles;
        [SerializeField] private ParticleSystem explosionParticles;
        [SerializeField] private AudioClip explosionSFX;

        [Header("Enemy Fire Settings")]
        [SerializeField] private float enemyFireDuration = 3f;

        private Vector3 _direction;
        private GameObject _player;
        private float _destroyTime;
        private ParticleSystem[] _fireballSystems;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _direction = _player.transform.forward.normalized;
            transform.position = _player.transform.position + _direction;
            _destroyTime = Time.time + lifetime;

            _fireballSystems = GetComponentsInChildren<ParticleSystem>();
        }

        private void Update()
        {
            if (Time.time >= _destroyTime)
                StopProjectile();

            transform.position += _direction * (speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) return;

            if (other.TryGetComponent<Target>(out var target))
            {
                target.TakeAttack(_direction, attack, knockback);
                if (other.TryGetComponent<Enemy>(out var enemy))
                    ApplyBurningEffect(enemy);
            }

            PlayExplosionEffects(transform.position);
            StopProjectile();
        }

        private void ApplyBurningEffect(Enemy enemy)
        {
            if (enemy.TryGetComponent<Target>(out var enemyTarget) && !enemyTarget.IsBurning)
            {
                if (enemyFireParticles != null)
                {
                    var fireEffect = Instantiate(enemyFireParticles, enemy.transform);
                    fireEffect.transform.localPosition = Vector3.zero;
                    enemyTarget.SetFireEffect(fireEffect);
                }
                enemyTarget.SetBurning(true, enemyFireDuration);
            }
        }

        private void PlayExplosionEffects(Vector3 position)
        {
            if (explosionSFX != null)
                AudioSource.PlayClipAtPoint(explosionSFX, position);

            if (explosionParticles != null)
            {
                var exp = Instantiate(explosionParticles, position, Quaternion.identity);
                var main = exp.main;
                float lifetime = main.duration + (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants
                    ? main.startLifetime.constantMax : main.startLifetime.constant);
                exp.Play();
                Destroy(exp.gameObject, lifetime);
            }
        }

        private void StopProjectile()
        {
            var col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            float maxLife = 0f;
            foreach (var ps in _fireballSystems)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                var main = ps.main;
                float startLife = (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                    ? main.startLifetime.constantMax : main.startLifetime.constant;
                maxLife = Mathf.Max(maxLife, main.duration + startLife);
            }

            Destroy(gameObject, maxLife);
        }
    }
}