using UnityEngine;

public class PlayerJumpingState : StateMachineBehaviour
{
    private PlayerID playerID;
    private PlayerMovement playerMovement;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerID = PlayerID.Instance;
        playerMovement = PlayerID.Instance.playerMovement;

        playerID.playerMovement.Jump(playerID.stateMachine.moveData.JumpForce);
        // playerMovement.SendMessage("DisableMovement"); // In case we want to disable movement while jumping
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        // playerMovement.SendMessage("EnableMovement"); // In case we want to disable movement while jumping
    }
}