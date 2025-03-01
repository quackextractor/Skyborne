using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float baseKnockback = 10f;
    [SerializeField] private float baseDamage = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Target target))
        {
            Vector3 attackOrigin = transform.position;
            Attack newAttack = new Attack(baseKnockback, baseDamage, attackOrigin);
            target.TakeAttack(newAttack);
        }
    }
}