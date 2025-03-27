using Unity.VisualScripting;
using UnityEngine;

public class Gust : Ability
{
    public override void AttackEffect()
    {
       
        if (!_isAttacking) StartCoroutine(Attack());
    }
}