using UnityEngine;

public class TestExternalEventTriggerer : ExternalEventTriggerer
{
    public override void TriggerExternalEvent()
    {
        Debug.Log("External event triggered!");
    }
}