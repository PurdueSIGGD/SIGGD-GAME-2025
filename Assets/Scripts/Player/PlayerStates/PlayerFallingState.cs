using UnityEngine;

public class PlayerFallingState : StateMachineBehaviour
{
    private PlayerID playerID;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerID = PlayerID.Instance;
    }
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        
        Vector2 moveInput = PlayerInput.Instance.movementInput;
        
        float speed = playerID.stateMachine.moveData.walkSpeed; // Falling speed is constant, no sprinting while falling.
        
        PlayerID.Instance.stateMachine.Run(moveInput, speed);
    }
}