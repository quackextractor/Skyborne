namespace Abilities
{
    public abstract class Ability : Weapon
    {
        public abstract override void AttackEffect();

        public override void StartAttack()
        {
        }
    }
}