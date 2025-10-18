using UnityEngine;

public class PlayerJumpingState : StateMachineBehaviour
{
    private PlayerID playerID;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerID = PlayerID.Instance;

        playerID.stateMachine.Jump(playerID.stateMachine.moveData.JumpForce);
    }
}