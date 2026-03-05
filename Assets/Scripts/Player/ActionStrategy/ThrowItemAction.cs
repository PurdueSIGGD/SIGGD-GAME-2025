using UnityEngine;

public class ThrowItemAction : IPlayerActionStrategy
{
    [Tooltip("Prefab for GameObject that will be thrown")]
    public GameObject projectile;
    [Tooltip("Force the projectile will be thrown with")]
    public float throwForce = 15f;
    protected override void OnEnter()
    {
        ThrowItem.Instance.Throw(projectile, throwForce);
        base.OnEnter();
        PlayHandAction(); // plays animation
        Inventory.Instance.Decrement();
    }
}
