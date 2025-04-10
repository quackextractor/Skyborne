using UnityEngine;

public class Fireball : Ability
{
    public GameObject firePrefab;
    
    protected void Start()
    {
       
    }

    private void Update()
    {
        
    }

    public override void AttackEffect()
    {
        Instantiate(firePrefab);
    }
}