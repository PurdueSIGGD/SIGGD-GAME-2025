using Sirenix.Serialization;
using UnityEngine;

public class InventoryDataSaveModule : ISaveModule
{
    private static string savePath = $"{FileManager.savesDirectory}/inventoryData";

    public static InventorySaveData inventoryData = new InventorySaveData();

    public static Inventory inventory;

    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;

        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        inventoryData = SerializationUtility.DeserializeValue<InventorySaveData>(bytes, DataFormat.Binary);

        return true;
    }

    public bool serialize()
    {
        if (inventory)
        {
            inventoryData.inventory = new InventorySaveData.SlotSaveData[Inventory.InventoryLength + Inventory.HotBarLength];
            Slot[] inventoryReference = inventory.getInventory();

            for (int i = 0; i < inventoryData.inventory.Length; i++)
            {
                Slot slot = inventoryReference[i];

                inventoryData.inventory[i] = new InventorySaveData.SlotSaveData
                {
                    itemInfo = slot.itemInfo,
                    count = slot.count
                };
            }
        }

        byte[] bytes = SerializationUtility.SerializeValue(inventoryData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}