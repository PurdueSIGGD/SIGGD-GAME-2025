using UnityEngine;

public class PlayerClimbingState : StateMachineBehaviour
{
    private PlayerStateMachine playerStateMachine;
    private ClimbAction climbActionScript = null;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerStateMachine = PlayerID.Instance.stateMachine;
        climbActionScript = PlayerID.Instance.gameObject.GetComponent<ClimbAction>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        /*
        Vector2 moveInput = PlayerInput.Instance.movementInput;
        bool isSprinting = PlayerInput.Instance.sprintInput;
        
        float speed = isSprinting ? playerStateMachine.moveData.sprintSpeed : 
            playerStateMachine.moveData.walkSpeed;
        
        PlayerID.Instance.stateMachine.Run(moveInput, speed);*/
        
        // climb action script is able to run
    }
}
