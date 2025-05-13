using Abilities;
using UnityEngine;

public class PlayerAttackScript : MonoBehaviour
{
    public const string Idle = "Idle";
    public const string Walk = "Walk";
    private const string Attack1 = "Attack 1";
    private const string Attack2 = "Attack 2";
    public Animator animator;
    private Ability[] abilities;
    private Weapon _weapon;
    private bool attackCount = true;
    private string currentAnimationState;

    private void Awake()
    {
        _weapon = GetComponentInChildren<Weapon>();
        abilities = GetComponentsInChildren<Ability>();

        if (_weapon == null) Debug.LogError("PlayerAttackScript: No Weapon component found in children!");
        if (abilities == null) Debug.LogError("PlayerAttackScript: No Ability component found in children!");
    }

    private void Update()
    {
        if (!_weapon.isAttacking)
            // Check for left mouse button press
            if (Input.GetMouseButtonDown(0))
                // Trigger the attack
                if (_weapon != null)
                {
                    _weapon.StartAttack();
                    if (attackCount)
                    {
                        ChangeAnimationState(Attack1);
                        attackCount = false;
                    }
                    else
                    {
                        ChangeAnimationState(Attack2);
                        attackCount = true;
                    }
                }


        if (Input.GetKeyDown(KeyCode.Q)) abilities[0].AttackEffect();
        if (Input.GetKeyDown(KeyCode.E)) abilities[1].AttackEffect();
    }

    private void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentAnimationState == newState)
        {
        }
        else
        {
            // PLAY THE ANIMATION //
            currentAnimationState = newState;
            animator.CrossFadeInFixedTime(currentAnimationState, _weapon.windUpTime);
            currentAnimationState = null;
        }
    }
    public void refreshInvetory() {
        abilities = GetComponentsInChildren<Ability>();
        foreach (Ability ability in abilities) {
            Debug.Log(ability.name);
        }
    }
}