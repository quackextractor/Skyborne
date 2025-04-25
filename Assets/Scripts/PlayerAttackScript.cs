using UnityEngine;

public class PlayerAttackScript : MonoBehaviour
{
    private Ability[] _ability;
    private Weapon _weapon;
   public Animator animator;
    string currentAnimationState;
    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";
    private bool attackCount = true;
    private void Awake()
    {
        _weapon = GetComponentInChildren<Weapon>();
        _ability = GetComponentsInChildren<Ability>();

        if (_weapon == null) Debug.LogError("PlayerAttackScript: No Weapon component found in children!");
        if (_ability == null) Debug.LogError("PlayerAttackScript: No Ability component found in children!");
    }

    private void Update()
    {
        if (!_weapon._isAttacking)
        {
            // Check for left mouse button press
            if (Input.GetMouseButtonDown(0))
                // Trigger the attack
                if (_weapon != null)
                {
                    _weapon.StartAttack();
                    if (attackCount)
                    {
                        ChangeAnimationState(ATTACK1);
                        attackCount = false;
                    }
                    else
                    {
                        ChangeAnimationState(ATTACK2);
                        attackCount = true;
                    }
                }
        }
        

        if (Input.GetKeyDown(KeyCode.Q)) _ability[0].AttackEffect();
        if (Input.GetKeyDown(KeyCode.E)) _ability[1].AttackEffect();
    }
    public void ChangeAnimationState(string newState)
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
}