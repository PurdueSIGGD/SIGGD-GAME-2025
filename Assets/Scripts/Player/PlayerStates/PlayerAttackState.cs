using UnityEngine;

public class PlayerAttackState : StateMachineBehaviour
{
    private PlayerStateMachine playerStateMachine;
    private IPlayerActionStrategy playerActionStrategy;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        playerStateMachine = PlayerID.Instance.stateMachine;

        var currentWeapon = playerStateMachine.GetEquippedWeapon();
        
        //playerActionStrategy = currentWeapon.actionStrategy;
        
        playerActionStrategy.Enter();
    }
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        playerActionStrategy.Update();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        
        playerActionStrategy.Exit();
    }
}