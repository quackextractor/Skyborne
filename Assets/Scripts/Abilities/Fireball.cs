using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Ability
{
    public GameObject go;
    private Rigidbody _rb;
    protected void Start()
    {
        _rb = go.GetComponent<Rigidbody>();
    }
    public override void SpecialEffect()
    {
        Instantiate(go);
        _rb = go.GetComponent<Rigidbody>();
        _rb.AddForce(go.transform.forward * 10, ForceMode.Impulse);

    }
    void Update()
    {
        _rb.AddForce(go.transform.forward * 10, ForceMode.Impulse);
    }
}
