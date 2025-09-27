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
        
        // TODO: Integrate with input manager (just read the move input from there)
        
        Vector2 moveInput = Vector2.one;   // moveDirection field from InputManager
        bool isSprinting = false;           // isSprinting field from InputManager
        
        float speed = isSprinting ? playerStateMachine.moveData.sprintSpeed : 
            playerStateMachine.moveData.walkSpeed;
        
        PlayerID.Instance.stateMachine.Run(moveInput, speed);
    }
}
