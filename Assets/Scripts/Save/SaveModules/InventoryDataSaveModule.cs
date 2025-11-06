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
        if (inventory == null) return false;

        inventoryData.inventory = new InventorySaveData.SlotSaveData[Inventory.InventoryLength + Inventory.HotBarLength];
        UISlot[] inventoryReference = inventory.GetInventory();

        for (int i = 0; i < inventoryData.inventory.Length; i++)
        {
            UISlot slot = inventoryReference[i];

            if (slot == null)
            {
                inventoryData.inventory[i] = new InventorySaveData.SlotSaveData
                {
                    name = "",
                    count = 0,
                    index = i
                };
            }
            else
            {
                inventoryData.inventory[i] = new InventorySaveData.SlotSaveData
                {
                    name = slot.itemInfo == null ? "" : slot.itemInfo.itemName.ToString(),
                    count = slot.count,
                    index = i
                };
            }
        }

        byte[] bytes = SerializationUtility.SerializeValue(inventoryData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}