using UnityEngine;

public class TriggerExternalEventExecutionStrategy : IQuestExecutionStrategy
{
    [SerializeField] public ExternalEventTriggerer externalEventTriggerer;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        externalEventTriggerer?.TriggerExternalEvent();
    }

    protected override void OnRestore()
    {
        base.OnRestore();
        externalEventTriggerer?.RestoreExternalEvent();
    }

    public override string ToString()
    {
        return "Trigger External Event";
    }
}

public abstract class ExternalEventTriggerer : MonoBehaviour
{
    public abstract void TriggerExternalEvent();

    /**
     * Called when the scene loads and this event already happened earlier (before a reload, or in a saved
     * game). Override to put back any lasting change to the scene. Does nothing by default.
     */
    public virtual void RestoreExternalEvent() { }
}