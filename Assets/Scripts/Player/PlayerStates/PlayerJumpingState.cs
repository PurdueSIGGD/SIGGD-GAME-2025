using UnityEngine;

public class PlayerJumpingState : StateMachineBehaviour
{
    private PlayerMovement playerMovement;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerMovement = PlayerID.Instance.playerMovement;
        playerMovement.Jump(playerMovement.moveData.JumpForce);
    }
}