using UnityEngine;

public class TriggerExternalEventExecutionStrategy : IQuestExecutionStrategy
{
    [SerializeField] public ExternalEventTriggerer externalEventTriggerer;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        externalEventTriggerer?.TriggerExternalEvent();
    }

    public override string ToString()
    {
        return "Trigger External Event";
    }
}

public abstract class ExternalEventTriggerer : MonoBehaviour
{
    public abstract void TriggerExternalEvent();
}