using UnityEngine;

/**
 * <summary>
 * The state machine behaviour for the player's moving state. If any movement input is detected, the
 * player will begin to run.
 * </summary>
 */
public class PlayerMovingState : StateMachineBehaviour
{
    private PlayerStateMachine playerStateMachine;
    private PlayerMovement playerMovement;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerMovement = PlayerID.Instance.playerMovement;
        playerMovement.IsMoving = true;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // Vector2 moveInput = PlayerInput.Instance.movementInput;
        // bool isSprinting = PlayerInput.Instance.sprintInput;

        // float speed = isSprinting ? playerStateMachine.moveData.sprintSpeed :
        //     playerStateMachine.moveData.walkSpeed;

        // Debug.Log("Moving with speed: " + speed);
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        playerMovement.IsMoving = false;
    }
}
