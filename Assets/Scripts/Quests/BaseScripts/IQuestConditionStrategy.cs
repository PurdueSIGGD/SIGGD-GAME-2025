using System;

/**
 * <summary>
 * Strategy pattern for defining when to broadcast quest events.
 * Allows for different implementations to determine the conditions under which a quest event should be broadcasted.
 * </summary>
 */
[Serializable]
public abstract class IQuestConditionStrategy
{
    
    protected QuestEventBroadcaster Broadcaster;
    /**
     * <summary>
     * Called when the strategy is started.
     * </summary>
     */
    public void Initialize(QuestEventBroadcaster b)
    {
        Broadcaster = b;
        OnInitialize();
    }

    /**
     * <summary>
     * Called every frame to determine whether to broadcast the event.
     * </summary>
     */
    public void Update() => OnUpdate();

    public void LateUpdate() => OnLateUpdate();
    
    public void Destroy() => OnDestroy();

    public void FixedUpdate() => OnFixedUpdate();
    
    /**
     * <summary>
     * Override this method to implement custom initialization logic.
     * </summary>
     */
    protected virtual void OnInitialize() { }

    
    /**
     * <summary>
     * Override this method to implement custom update logic.
     * </summary>
     */
    protected virtual void OnUpdate()
    {
        
    }
    
    /**
     * <summary>
     * Override this method to implement custom late update logic.
     * </summary>
     */
    protected virtual void OnLateUpdate()
    {

    }

    /**
     * <summary>
     * Override this method to implement custom fixed update logic.
     * </summary>
     */
    protected virtual void OnFixedUpdate()
    {

    }
    
    /**
     * <summary>
     * Override this method to implement custom destruction logic.
     * </summary>
     */
    protected virtual void OnDestroy() { }
    
    /**
     * <summary>
     * Broadcasts the event using the provided broadcaster.
     * </summary>
     */
    protected void Broadcast(QuestEventBroadcaster broadcaster)
    {
        broadcaster.Broadcast();
    }

    /**
     * <summary>
     * Evaluates the condition to determine if the event has been triggered or deactivated.
     * </summary>
     * <returns>True if the condition is met, false otherwise.</returns>
     */
    public abstract bool Evaluate();

    /**
     * <summary>
     * Indicates whether the strategy should continue to check the condition after it has been triggered.
     * </summary>
     *
     * <returns>True if the strategy should stop checking after being triggered, false otherwise.</returns>
     */
    public abstract bool StopIfTriggered();
}