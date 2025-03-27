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
        
    }

    public override void AttackEffect()
    {
        Instantiate(go);
    }
    private void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (!_isAttacking) StartCoroutine(Attack());
    }
}