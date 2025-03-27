using UnityEngine;

public class Fireball : Ability
{
    public GameObject go;
    private Rigidbody _rb;

    protected void Start()
    {
        _rb = go.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _rb.AddForce(go.transform.forward * 10, ForceMode.Impulse);
    }

    public override void AttackEffect()
    {
        Instantiate(go);
        _rb = go.GetComponent<Rigidbody>();
        _rb.AddForce(go.transform.forward * 10, ForceMode.Impulse);
    }
}