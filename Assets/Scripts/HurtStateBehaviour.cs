using UnityEngine;

/// <summary>
/// Attach this StateMachineBehaviour to the "Hurt" state in your Animator.
/// When the Hurt clip finishes, it will notify the EnemyAnimationController to resume walking.
/// </summary>
public class HurtStateBehaviour : StateMachineBehaviour
{
    // This is called when exiting the Hurt state (after the animation has played)
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var controller = animator.GetComponent<EnemyAnimationController>();
        if (controller != null)
        {
            controller.OnHurtFinished();
        }
    }
}