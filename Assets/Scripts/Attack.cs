using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Attack
{
    public float knockbackValue;
    public float damageValue; // Added field
    public Vector3 originPosition;

    public Attack(float knockback, float damage, Vector3 origin)
    {
        knockbackValue = knockback;
        damageValue = damage; // Assign damage
        originPosition = origin;
    }
}