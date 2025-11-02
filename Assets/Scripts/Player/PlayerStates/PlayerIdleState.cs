using UnityEngine;

/**
 * <summary>
 * The state machine behaviour for the player's idle state. If no movement input is detected, the
 * player will remain idle.
 * </summary>
 */
public class PlayerIdleState : StateMachineBehaviour
{
    private PlayerID playerID;
    private PlayerMovement playerMovement;

    // currently the idle state just does what the moving state does, but maybe there needs to be something special
    // for when the player is idle so this state should still exist.

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerID = PlayerID.Instance;
        playerMovement = PlayerID.Instance.playerMovement;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (PlayerID.Instance == null)
        {
            Debug.LogError("PlayerID.Instance is null!");
            return;
        }

        if (PlayerID.Instance.playerMovement == null)
        {
            Debug.LogError("PlayerID.Instance.playerMovement is null!");
            return;
        }

        if (PlayerInput.Instance == null)
        {
            Debug.LogError("PlayerInput.Instance is null!");
            return;
        }

        //Vector2 moveInput = PlayerInput.Instance.movementInput;
        //bool isSprinting = PlayerInput.Instance.sprintInput;

        //float speed = isSprinting ? playerID.stateMachine.moveData.sprintSpeed :
        //    playerID.stateMachine.moveData.walkSpeed;

        //PlayerID.Instance.playerMovement.Run(moveInput, speed);
    }
}