using System;
using UnityEngine;

[Serializable]
public class InventorySaveData
{
    [SerializeReference] public SlotSaveData[] inventory;

    [Serializable]
    public struct SlotSaveData
    {
        [SerializeReference] public ItemInfo itemInfo;
        public int count;
    }
}
