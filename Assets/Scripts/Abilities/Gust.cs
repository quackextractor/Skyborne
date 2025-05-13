using UnityEngine;

namespace Abilities
{
    public class Gust : Ability
    {
        public GameObject gameObject;
  
        public override void AttackEffect()
        {
            
            if (!isAttacking) { 
                StartCoroutine(Attack());
            };
           

        }
    }
}