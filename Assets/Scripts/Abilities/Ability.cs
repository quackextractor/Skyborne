namespace Abilities
{
    public abstract class Ability : Weapon
    {
        bool bought = false;

        public bool Bought { get => bought; set => bought = value; }

        public abstract override void AttackEffect();

        public override void StartAttack()
        {
        }
        
    }
}