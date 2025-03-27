using UnityEngine;

public class PlayerAttackScript : MonoBehaviour
{
    private Ability[] _ability;
    private Weapon _weapon;

    private void Awake()
    {
        _weapon = GetComponentInChildren<Weapon>();
        _ability = GetComponentsInChildren<Ability>();

        if (_weapon == null) Debug.LogError("PlayerAttackScript: No Weapon component found in children!");
        if (_ability == null) Debug.LogError("PlayerAttackScript: No Ability component found in children!");
    }

    private void Update()
    {
        // Check for left mouse button press
        if (Input.GetMouseButtonDown(0))
            // Trigger the attack
            if (_weapon != null)
                _weapon.StartAttack();

        if (Input.GetKeyDown(KeyCode.Q)) _ability[0].AttackEffect();
        if (Input.GetKeyDown(KeyCode.E)) _ability[1].AttackEffect();
    }
}