using UnityEngine;

public class SpawnItemActionStrategy : IPlayerActionStrategy
{
    public GameObject prefab;

    protected override void OnEnter()
    {
        base.OnEnter();
        // GameObject.Instantiate(prefab);

        // get currently selected item from inventory
        ItemInfo selectedItem = Inventory.Instance.GetSelectedItem();

        // check if the selected item is a trap and has valid prefabs
        if (selectedItem.itemType == ItemInfo.ItemType.Trap && selectedItem.itemPrefab != null && selectedItem.itemPlacementPrefab != null)
        {
            ObjectPlacer.Instance.StartPlacement(selectedItem);
        }

        // Inventory.Instance.Decrement();
    }
}
