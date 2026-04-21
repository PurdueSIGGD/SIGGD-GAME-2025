using UnityEngine;

public class ThrowBaitAction : IPlayerActionStrategy
{
    [Tooltip("Prefab for GameObject that will be thrown")]
    public GameObject projectile;
    [Tooltip("Force the projectile will be thrown with")]
    public float throwForce = 15f;
    [Tooltip("Radius of the bait's effect")]
    public float radius = 10f;
    [Tooltip("Duration of the bait's effect in seconds")]
    public float duration = 5f;
    [Tooltip("Material for the bait projectile")]
    public Material material;

    [Tooltip("Duration of the bait's destroy time in seconds")]
    public float baitDuration = 3f;
    protected override void OnEnter()
    {
        ThrowItem.Instance.ThrowBait(projectile, throwForce, radius, duration, baitDuration, material);
        base.OnEnter();
        PlayHandAction(); // plays animation
        Inventory.Instance.Decrement();
    }
}
