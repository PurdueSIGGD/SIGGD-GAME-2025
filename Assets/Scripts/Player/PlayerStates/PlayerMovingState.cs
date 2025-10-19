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
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerStateMachine = PlayerID.Instance.stateMachine;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        Vector2 moveInput = PlayerInput.Instance.movementInput;
        bool isSprinting = PlayerInput.Instance.sprintInput;
        
        float speed = isSprinting ? playerStateMachine.moveData.sprintSpeed : 
            playerStateMachine.moveData.walkSpeed;
        
        PlayerID.Instance.stateMachine.Run(moveInput, speed);
    }
    
}