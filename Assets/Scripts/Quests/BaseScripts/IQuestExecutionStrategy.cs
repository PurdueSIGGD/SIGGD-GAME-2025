using System;

/**
 * Strategy pattern for defining how quest event receivers handle triggering events (spawn something, shake player, etc)
 */
[Serializable]
public abstract class IQuestExecutionStrategy
{
    protected QuestEventReceiver questEventReceiver;
    
    public void Initialize(QuestEventReceiver receiver)
    {
        questEventReceiver = receiver;
        OnInitialize();
    }
    
    /**
     * Override this method to implement custom initialization logic. OnDeactivate is also called before initialization
     * to ensure any previous state is cleaned up.
     */
    protected virtual void OnInitialize() { }
    
    public void Update() => OnUpdate();
    
    /**
     * Override this method to implement custom update logic.
     */
    protected virtual void OnUpdate() { }
    
    public void LateUpdate() => OnLateUpdate();
    
    /**
     * Override this method to implement custom late update logic.
     */
    protected virtual void OnLateUpdate() { }
    
    public void FixedUpdate() => OnFixedUpdate();
    
    /**
     * Override this method to implement custom fixed update logic.
     */
    protected virtual void OnFixedUpdate() { }
    
    public void Deactivate() => OnDeactivate();
    
    /**
     * Override this method to implement custom deactivation logic. Called immediately before initialization
     * as well, so ensure any previous state is cleaned up.
     */
    protected virtual void OnDeactivate() { }
}