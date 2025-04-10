using UnityEngine;

public class PlayerAttackScript : MonoBehaviour
{
    private Ability[] _ability;
    private Weapon _weapon;
   public Animator animator;
    string currentAnimationState;
    public const string ATTACK1 = "Attack 1";

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
            {
                ChangeAnimationState(ATTACK1);
                _weapon.StartAttack();
               
            }
        if (Input.GetKeyDown(KeyCode.Q)) _ability[0].AttackEffect();
        if (Input.GetKeyDown(KeyCode.E)) _ability[1].AttackEffect();
    }
    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
       if (currentAnimationState == newState) return;
        Debug.Log("meow");
        // PLAY THE ANIMATION //
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
        currentAnimationState = null;
    }
}