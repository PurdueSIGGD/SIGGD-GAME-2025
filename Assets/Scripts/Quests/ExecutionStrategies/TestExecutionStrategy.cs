using UnityEngine;

public class TestExecutionStrategy : IQuestExecutionStrategy
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Debug.Log("TestExecutionStrategy Initialized");
    }

    public override string ToString()
    {
        return "Test Execution Strategy";
    }
}