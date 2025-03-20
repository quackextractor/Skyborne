using UnityEngine;

public class Gust : Ability
{
    public override void SpecialEffect()
    {
        Debug.Log("emwo");
        if (!_isAttacking) StartCoroutine(Attack());
    }
}