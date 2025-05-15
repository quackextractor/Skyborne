using System.Drawing;
using UnityEngine;

namespace Abilities
{
    public abstract class Ability : Weapon
    {
        bool bought = false;
        [SerializeField]  private Sprite abilityImage;
        [SerializeField] private int cost;

        public bool Bought { get => bought; set => bought = value; }
        public Sprite AbilityImage { get => abilityImage; set => abilityImage = value; }
        public int Cost { get => cost; set => cost = value; }

        public abstract override void AttackEffect();

        public override void StartAttack()
        {
        }
        
    }
}