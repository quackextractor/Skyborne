namespace Abilities
{
    public abstract class Ability : Weapon
    {
        bool bought = false;
        public abstract override void AttackEffect();

        public override void StartAttack()
        {
        }
    }
}