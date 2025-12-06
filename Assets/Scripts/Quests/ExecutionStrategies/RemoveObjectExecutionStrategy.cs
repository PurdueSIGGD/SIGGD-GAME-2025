using UnityEngine;
using System;
public class RemoveObjectExecutionStrategy : IQuestExecutionStrategy
{

    public GameObject destructable;
    protected override void OnInitialize()
    {
        base.OnInitialize();
        UnityEngine.Object.Destroy(destructable);
        Debug.Log("hihihihihyayayayayaayayhihi");
    }

    public override string ToString()
    {
        return "Test Execution Strategy";
    }
}