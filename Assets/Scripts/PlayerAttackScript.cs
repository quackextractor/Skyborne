using Abilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackScript : MonoBehaviour
{
    public const string Idle = "Idle";
    public const string Walk = "Walk";
    private const string Attack1 = "Attack 1";
    private const string Attack2 = "Attack 2";
    public Animator animator;
    private Ability[] abilities = new Ability[10];
    private Weapon _weapon;
    private bool attackCount = true;
    private string currentAnimationState;
    public GameObject sliderParent;
    private Slider[] sliders; 
    private void Awake()
    {
        sliders = sliderParent.GetComponentsInChildren<Slider>();
        _weapon = GetComponentInChildren<Weapon>();
       // abilities = GetComponentsInChildren<Ability>();
       
        if (_weapon == null) Debug.LogError("PlayerAttackScript: No Weapon component found in children!");
      //  if (abilities == null) Debug.LogError("PlayerAttackScript: No Ability component found in children!");
    }

    private void Update()
    {
        if (!_weapon.isAttacking) {
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
        }
        foreach (Slider slider in sliders) {
            slider.value -= Time.deltaTime; 
        }

            if (abilities[0] != null && Input.GetKeyDown(KeyCode.Q)&& !abilities[0].isAttacking)
        {
            abilities[0].AttackEffect();
            sliders[0].value = sliders[0].maxValue;
        }

        if (abilities[1] != null && Input.GetKeyDown(KeyCode.E) && !abilities[1].isAttacking)
        {
            abilities[1].AttackEffect();
            sliders[1].value = sliders[1].maxValue;
        }


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
            animator.CrossFadeInFixedTime(currentAnimationState, _weapon.WindUpTime);
            currentAnimationState = null;
        }
    }
    public void RefreshInvetory() {
        abilities = GetComponentsInChildren<Ability>();
        foreach (Ability ability in abilities) {
            Debug.Log(ability.name);
        }
    }

    //yucky code
    public void SelectQAbility(Ability a) {
        sliders[0].maxValue= a.CooldownTime + a.WindUpTime + a.ActivationTime;

        if (abilities[1] != a)
        {
            if (abilities[0] != null)
            {
                abilities[0].gameObject.SetActive(false);
            }
            abilities[0] = a;
            abilities[0].gameObject.SetActive(true);
        }
    }
    public void SelectEAbility(Ability a)
    {
        sliders[1].maxValue = a.CooldownTime + a.WindUpTime + a.ActivationTime;
        if (abilities[0] != a)
        {
            if (abilities[1] != null)
            {
                abilities[1].gameObject.SetActive(false);
            }
            abilities[1] = a;
            abilities[1].gameObject.SetActive(true);
        }
    }
}