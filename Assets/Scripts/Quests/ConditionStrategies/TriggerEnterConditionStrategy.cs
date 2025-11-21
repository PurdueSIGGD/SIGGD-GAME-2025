using UnityEngine;

public class TriggerEnterConditionStrategy : IQuestConditionStrategy
{
    public TriggerListener triggerListener;
    
    [Tooltip("If true, the condition will deactivate when the player exits the trigger.")]
    public bool deactivateOnExit = true;
    [Tooltip("If true, the condition will be true when the player is NOT in the trigger.")]
    public bool inverse = false;
    
    private bool inTrigger = false;
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        triggerListener.onTriggerEnter += OnTriggerEnter;
        triggerListener.onTriggerExit += OnTriggerExit;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        triggerListener.onTriggerEnter -= OnTriggerEnter;
        triggerListener.onTriggerExit -= OnTriggerExit;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerID pc))
        {
            inTrigger = false;
            return;
        }
        
        inTrigger = true;
        
        Broadcast(Broadcaster);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!deactivateOnExit)
        {
            return;
        }
        
        if (!other.TryGetComponent(out PlayerID pc))
        {
            return;
        }
        
        inTrigger = false;
        
        Broadcast(Broadcaster);
    }

    public override string ToString()
    {
        return "Trigger Enter";
    }
    
    public override bool Evaluate() => inTrigger != inverse;
    
    public override bool StopIfTriggered() => false;
}