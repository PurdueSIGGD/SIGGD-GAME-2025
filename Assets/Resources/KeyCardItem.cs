using UnityEngine;

public class KeyCardItemAction : IPlayerActionStrategy
{
    protected override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("player used the keycard");
    }
}
