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
    
    // currently the idle state just does what the moving state does, but maybe there needs to be something special
    // for when the player is idle so this state should still exist.
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerID = PlayerID.Instance;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        
        Vector2 moveInput = PlayerInput.Instance.movementInput;
        bool isSprinting = PlayerInput.Instance.sprintInput;
        
        float speed = isSprinting ? playerID.stateMachine.moveData.sprintSpeed : 
            playerID.stateMachine.moveData.walkSpeed;
        
        PlayerID.Instance.stateMachine.Run(moveInput, speed);
    }
}
