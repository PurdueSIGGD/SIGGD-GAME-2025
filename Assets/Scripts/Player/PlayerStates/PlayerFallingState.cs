using Unity.VisualScripting;
using UnityEngine;

public class PlayerFallingState : StateMachineBehaviour
{
    private PlayerMovement playerMovement;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerMovement = PlayerID.Instance.playerMovement;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}