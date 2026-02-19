using UnityEngine;

public class RockItemAction : IPlayerActionStrategy
{
    protected override void OnEnter()
    {
        base.OnEnter();
        PlayHandAction(); // plays animation for apple, but this is instant rn so it does nothing
        Inventory.Instance.Decrement();
        Debug.Log("player threw a rock");
    }
}
