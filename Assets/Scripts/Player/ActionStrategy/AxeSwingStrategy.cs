using UnityEngine;

/**
 * <summary>
 * Strategy for axe swinging action.
 * </summary>
 */
public class AxeSwingStrategy : IPlayerActionStrategy
{
    public AxeSwingStrategy(PlayerStateMachine stateMachine, AnimationClip animationClip)
    {
        this.stateMachine = stateMachine;
        this.animationClip = animationClip;
    }
    
    protected override void OnEnter()
    {
        base.OnEnter();
        // Additional logic for entering axe swing state
    }

    protected override void OnUpdate()
    {
        // Logic for updating axe swing state
    }

    protected override void OnExit()
    {
        // Logic for exiting axe swing state
    }
    
}