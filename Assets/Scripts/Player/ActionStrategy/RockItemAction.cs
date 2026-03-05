using UnityEngine;

public class RockItemAction : IPlayerActionStrategy
{
    private Camera playerCam;
    [Tooltip("Prefab for GameObject that will be thrown")]
    public GameObject projectile;
    protected override void OnEnter()
    {
        RockThrow.Instance.ThrowRock(projectile);
        base.OnEnter();
        PlayHandAction(); // plays animation
        Inventory.Instance.Decrement();
    }
}
