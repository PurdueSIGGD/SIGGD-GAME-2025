
using Extensions.EventBus;

public class PlayerListener
{
    public bool IsCrouching { get; private set; }
    public bool IsSprinting { get; private set; }

    private readonly float sprintRangeMultiplier;
    private readonly float crouchRangeMultiplier;
    
    public float CrouchRangeMultiplier => IsCrouching ? crouchRangeMultiplier : 1f;
    public float SprintRangeMultiplier => IsSprinting ? sprintRangeMultiplier : 1f;
    
    private EventBinding<OnPlayerActionEvent> playerActionEventBinding;
    
    public PlayerListener(float crouchRangeMultiplier, float sprintRangeMultiplier)
    {
        this.crouchRangeMultiplier = crouchRangeMultiplier;
        this.sprintRangeMultiplier = sprintRangeMultiplier;
        
        playerActionEventBinding = new EventBinding<OnPlayerActionEvent>((e) =>
        {
            IsCrouching = e.IsCrouching;
            IsSprinting = e.IsSprinting;
        });
        
        EventBus<OnPlayerActionEvent>.Register(playerActionEventBinding);
    }
    
    public void DisableListener()
    {
        EventBus<OnPlayerActionEvent>.Deregister(playerActionEventBinding);
    }
}

public struct OnPlayerActionEvent : IEvent
{
    public readonly bool IsCrouching;
    public readonly bool IsSprinting;
    
    public OnPlayerActionEvent(bool isCrouching, bool isSprinting)
    {
        IsCrouching = isCrouching;
        IsSprinting = isSprinting;
    }
}