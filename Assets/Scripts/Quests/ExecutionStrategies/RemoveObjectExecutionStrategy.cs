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

    protected override void OnRestore()
    {
        base.OnRestore();
        // the object was already removed before the scene reloaded, keep it removed
        if (destructable != null) UnityEngine.Object.Destroy(destructable);
    }

    public override string ToString()
    {
        return "Test Execution Strategy";
    }
}