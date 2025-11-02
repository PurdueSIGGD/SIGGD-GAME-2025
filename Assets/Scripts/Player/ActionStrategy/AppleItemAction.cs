using UnityEngine;

public class AppleItem : IPlayerActionStrategy
{
    protected override void OnEnter()
    {
        base.OnEnter();
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
        Inventory.Instance.Decrement();
        Debug.Log("player ate an apple");
    }
}
