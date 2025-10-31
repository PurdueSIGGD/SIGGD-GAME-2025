using UnityEngine;

/**
 * <summary>
 * The state machine behaviour for the player's moving state. If any movement input is detected, the
 * player will begin to run.
 * </summary>
 */
public class PlayerMovingState : StateMachineBehaviour
{
    private PlayerMovement playerMovement;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerMovement = PlayerID.Instance.playerMovement;
        playerMovement.IsMoving = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        playerMovement.IsMoving = false;
    }

}
