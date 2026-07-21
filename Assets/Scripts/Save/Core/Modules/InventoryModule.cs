using System;
using Sirenix.Serialization;
using UnityEngine;

namespace SIGGD.Save.Modules
{
    /// <summary>
    /// Player inventory (hotbar + main inventory) and the selected slot index. Ported from
    /// <c>InventoryDataSaveModule</c> — but without the old module's static-field handshake with
    /// <c>Inventory.Start</c>. The live inventory is now populated from a call to
    /// <c>Inventory.Instance.LoadFromSaveData(...)</c> triggered by <see cref="Apply"/>.
    /// </summary>
    public class InventoryModule : ISaveModule
    {
        public string Key => "inventory";
        public SaveScope Scope => SaveScope.Gameplay;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        /// <summary>The in-memory POCO. Always non-null after construction.</summary>
        public InventorySaveData Data { get; private set; } = new();

        public void Capture()
        {
            var inv = Inventory.Instance;
            if (inv == null)
            {
                Debug.Log("InventoryModule: no Inventory in scene — capture skipped.");
                return;
            }

            InventorySlot[] slots = inv.GetInventory();
            int len = slots?.Length ?? 0;
            Data.selected = inv.GetSelected();
            Data.inventory = new InventorySaveData.SlotSaveData[len];

            for (int i = 0; i < len; i++)
            {
                InventorySlot slot = slots[i];
                if (slot == null || slot.itemInfo == null)
                {
                    Data.inventory[i] = new InventorySaveData.SlotSaveData { name = "Empty", count = 0, index = i };
                }
                else
                {
                    Data.inventory[i] = new InventorySaveData.SlotSaveData
                    {
                        name = slot.itemInfo.itemName.ToString(),
                        count = slot.count,
                        index = i,
                    };
                }
            }
        }

        public void Apply()
        {
            if (!IsLoaded)
            {
                // No real save on disk — leave the inventory at whatever defaults Inventory.Start set.
                return;
            }

            var inv = Inventory.Instance;
            if (inv == null)
            {
                Debug.Log("InventoryModule: no Inventory in scene — apply skipped.");
                return;
            }
            if (Data == null || Data.inventory == null)
            {
                return;
            }
            inv.LoadFromSaveData(Data);
        }

        public byte[] Serialize() => SerializationUtility.SerializeValue(Data, DataFormat.Binary);

        public void Deserialize(byte[] bytes, int version)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Data = new InventorySaveData();
                IsLoaded = false;
                return;
            }
            try
            {
                Data = SerializationUtility.DeserializeValue<InventorySaveData>(bytes, DataFormat.Binary) ?? new InventorySaveData();
                IsLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"InventoryModule: deserialize failed (v{version}), resetting to defaults: {e}");
                Data = new InventorySaveData();
                IsLoaded = false;
            }
        }
    }
}
