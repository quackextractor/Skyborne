using UnityEngine;

namespace Abilities
{
    public class Fireball : Ability
    {
        public GameObject firePrefab;

        public override void AttackEffect()
        {
            Instantiate(firePrefab);
            
        }
    }
}