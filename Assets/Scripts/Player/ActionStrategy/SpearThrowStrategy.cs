using System;
using UnityEngine;

/**
 * <summary>
 * Strategy for spear throwing action.
 * </summary>
 */
[Serializable]
public class SpearThrowStrategy : IPlayerActionStrategy
{
    public float throwDistance = 10f;
    
    protected override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Spear throw");
        // Additional logic for entering spear throw state
    }

    protected override void OnUpdate()
    {
        // Logic for updating spear throw state
    }

    protected override void OnExit()
    {
        // Logic for exiting spear throw state
    }
}