using UnityEngine;

/**
 * <summary>
 * The IPlayerActionStrategy interface defines the structure for player action strategies. All player action behaviors
 * will implement this interface, so the PlayerAttackState can use them interchangeably. This will be serialized in
 * a player weapon scriptable object to define different attack behaviors for different weapons.
 *
 * USAGE: Create concrete classes implementing this interface for each specific player action (e.g., AxeSwingStrategy,
 * SpearThrowStrategy). Make sure to mark the class as [Serializable] and DO NOT define a constructor (or make sure
 * to have a parameterless one), so Unity can serialize it properly.
 * </summary>
 */
public abstract class IPlayerActionStrategy
{
    protected PlayerStateMachine stateMachine;
    public AnimationClip animationClip;
    public float ActionDuration => animationClip != null ? animationClip.length : 0f;
    private float actionTimer = 0;
    
    /**
     * Called when the action state is entered. Plays the associated animation clip.
     */
    protected virtual void OnEnter()
    {
        if (animationClip != null)
            stateMachine.animator.Play(animationClip.name);
    }
    
    /**
     * Called every frame while in the action state. Can be used to handle timing or transitions.
     */
    protected virtual void OnUpdate() { }
    
    /**
     * Called when the action state is exited. Can be used for cleanup or resetting variables.
     */
    protected virtual void OnExit() { }
    
    /**
     * Staying consistent with the template design pattern, we define Enter, Update, and Exit methods that call the
     * corresponding protected methods. This allows subclasses to override the protected methods while keeping
     * the public interface consistent.
     */
    public void Enter()
    {
        stateMachine = PlayerID.Instance.stateMachine;
        OnEnter();
    }

    /**
     * Template design pattern to always evaluate timing logic in the Update method, while letting subclasses define
     * specific update behavior.
     */
    public void Update()
    {
        actionTimer += Time.deltaTime;
        
        OnUpdate();
        
        if (actionTimer >= ActionDuration)
        {
            stateMachine.animator.SetBool(Animator.StringToHash("isAttacking"), false);
            actionTimer = 0;
        }
    }
    
    public void Exit() => OnExit();
}