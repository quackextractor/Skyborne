using System.Collections;
using UnityEngine;

namespace Abilities
{
    public class Fireball : Ability
    {
        public GameObject firePrefab;

        public override void AttackEffect()
        {
            StartCoroutine(Attack());
            
        }
        protected IEnumerator Attack()
        {
            isAttacking = true;
            _hitTargets.Clear();

            yield return new WaitForSeconds(WindUpTime);

            // Instantiate fire projectile
            Instantiate(firePrefab, transform.position, transform.rotation);

            weaponCollider.enabled = true;
            yield return new WaitForSeconds(ActivationTime);
            weaponCollider.enabled = false;
            yield return new WaitForSeconds(CooldownTime);

            isAttacking = false;
        }
    }
}