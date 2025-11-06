using System;
using UnityEngine;

[Serializable]
public class InventorySaveData
{
    [SerializeReference] public SlotSaveData[] inventory;

    [Serializable]
    public struct SlotSaveData
    {
        public string name;
        public int count;
        public int index;
    }
}
