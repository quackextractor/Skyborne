using System;
using UnityEngine;

[Serializable]
public class Attack
{
    public float knockbackValue;
    public float damageValue;
    public Vector3 originPosition;

    public Attack(float knockback, float damage, Vector3 origin)
    {
        knockbackValue = knockback;
        damageValue = damage;
        originPosition = origin;
    }
}