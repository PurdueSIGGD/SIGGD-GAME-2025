using UnityEngine;

/**
 * <summary>
 * Strategy for spear throwing action.
 * </summary>
 */
public class SpearThrowStrategy : IPlayerActionStrategy
{
    public SpearThrowStrategy(PlayerStateMachine stateMachine, AnimationClip animationClip)
    {
        this.stateMachine = stateMachine;
        this.animationClip = animationClip;
    }
    
    protected override void OnEnter()
    {
        base.OnEnter();
        // Additional logic for entering spear throw state
    }

    protected override void OnUpdate()
    {
        // Logic for updating spear throw state
    }

    protected override void OnExit()
    {
        // Logic for exiting spear throw state
    }
}