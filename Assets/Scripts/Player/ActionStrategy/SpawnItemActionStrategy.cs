using UnityEngine;

public class SpawnItemActionStrategy : IPlayerActionStrategy
{
    public GameObject prefab;

    protected override void OnEnter()
    {
        base.OnEnter();
        // GameObject.Instantiate(prefab);

        if (Inventory.Instance.GetSelectedItem().itemType == ItemInfo.ItemType.Trap)
        {
            ObjectPlacer.Instance.startPlaceMode = true;
        }

        // Inventory.Instance.Decrement();
    }
}
