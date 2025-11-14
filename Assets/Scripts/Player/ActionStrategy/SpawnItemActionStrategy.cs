using UnityEngine;

public class SpawnItemActionStrategy : IPlayerActionStrategy
{
    public GameObject prefab;

    protected override void OnEnter()
    {
        Debug.LogWarning("SpawnItemActionStrategy OnEnter called");

        base.OnEnter();
        // GameObject.Instantiate(prefab);

        if (Inventory.Instance.GetSelectedItem().itemType == ItemInfo.ItemType.Trap)
        {
            Debug.LogWarning("Starting Place Mode");
            ObjectPlacer.Instance.startPlaceMode = true;
        }

        // Inventory.Instance.Decrement();
    }
}
