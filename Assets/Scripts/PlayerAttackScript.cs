using UnityEngine;

public class PlayerAttackScript : MonoBehaviour
{
    private Weapon _weapon;

    private void Awake()
    {
        _weapon = GetComponentInChildren<Weapon>();
        
        if (_weapon == null)
        {
            Debug.LogError("PlayerAttackScript: No Weapon component found in children!");
        }
    }

    private void Update()
    {
        // Check for left mouse button press
        if (Input.GetMouseButtonDown(0))
        {
            // Trigger the attack
            if (_weapon != null)
            {
                _weapon.StartAttack();
            }
        }
    }
}