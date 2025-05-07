namespace Abilities
{
    public class Gust : Ability
    {
        public override void AttackEffect()
        {
            if (!isAttacking) StartCoroutine(Attack());
        }
    }
}