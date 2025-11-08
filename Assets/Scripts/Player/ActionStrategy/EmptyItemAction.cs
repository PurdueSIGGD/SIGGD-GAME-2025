using UnityEngine;

public class EmptyItemAction : IPlayerActionStrategy
{
    protected override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Player attempting to use empty item");
    }
}
