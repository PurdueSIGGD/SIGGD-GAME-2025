using UnityEngine;

public class PlayerFallingState : StateMachineBehaviour
{
    private PlayerID playerID;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerID = PlayerID.Instance;
    }
}