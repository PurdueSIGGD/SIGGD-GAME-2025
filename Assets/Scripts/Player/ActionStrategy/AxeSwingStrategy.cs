using System;
using UnityEngine;

/**
 * <summary>
 * Strategy for axe swinging action.
 * </summary>
 */
[Serializable]
public class AxeSwingStrategy : IPlayerActionStrategy
{
    protected override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Axe Swing");
        // Additional logic for entering axe swing state
    }

    protected override void OnUpdate()
    {
        // Logic for updating axe swing state
    }

    protected override void OnExit()
    {
        // Logic for exiting axe swing state
    }
    
}