using System.Collections;
using UnityEngine;

namespace Abilities
{
    public class Gust : Ability
    {
        [Header("Particle Settings")]
        public GameObject GustParticlesGameObject;
        public float emissionDuration = 1.0f; // how long the gust runs

        private ParticleSystem ps;

        private void Awake()
        {
            // cache the ParticleSystem
            ps = GustParticlesGameObject.GetComponent<ParticleSystem>();
            if (ps == null)
                Debug.LogError("Gust: no ParticleSystem on GustParticlesGameObject!");
        }

        public override void AttackEffect()
        {
            if (!isAttacking)
                StartCoroutine(DoGust());
        }

        private IEnumerator DoGust()
        {
            isAttacking = true;

            // start emitting
            ps.Play();

            // wait
            yield return new WaitForSeconds(emissionDuration);

            // stop emitting new particles, but let existing ones finish
            ps.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);

            // (optional) if you want to immediately clear all particles instead:
            // ps.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);

            isAttacking = false;
        }
    }
}