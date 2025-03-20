using UnityEngine;

public class Gust : Ability
{
    public override void SpecialEffect()
    {
        if (!_isAttacking) StartCoroutine(Attack());
    }
}