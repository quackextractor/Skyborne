using UnityEngine;

public class Fireball : Ability
{
    public GameObject go;
    
    protected void Start()
    {
        Debug.Log("uh oh");
    }

    private void Update()
    {
        
    }

    public override void AttackEffect()
    {
        Instantiate(go);
    }
}