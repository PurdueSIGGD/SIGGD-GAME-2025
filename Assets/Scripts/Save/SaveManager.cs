using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{

    [SerializeField] private Inventory inventory; //TEMP
    InventoryDataSaveModule inventoryModule;
    PlayerDataSaveModule playerModule;

    private ISaveModule[] modules;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        inventoryModule =new InventoryDataSaveModule();
        playerModule = gameObject.AddComponent<PlayerDataSaveModule>();

        InventoryDataSaveModule.inventory = inventory; // TEMP

        modules = new ISaveModule[] {inventoryModule, playerModule};

        Load();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public bool Load()
    {
        foreach (var module in modules)
            module.deserialize();

        return true;
    }

    public bool Save()
    {
        foreach (var module in modules)
            module.serialize();
        return true;
    }
}
