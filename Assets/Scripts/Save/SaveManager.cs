using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public List<ISaveModule> modules;
    [SerializeField] private Inventory inventory; //TEMP

    public ISaveModule[] modules = {
        new PlayerDataSaveModule(),
        new InventoryDataSaveModule()
    };

    protected override void Awake()
    {
        base.Awake();
        Load();
    }

    void Start()
    {
        InventoryDataSaveModule.inventory = inventory; // TEMP
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public bool Load()
    {
        modules = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                  .OfType<ISaveModule>()
                  .ToList();
        foreach (var module in modules)
            module.deserialize();

        return true;
    }

    public bool Save()
    {
        modules = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                  .OfType<ISaveModule>()
                  .ToList();
        foreach (var module in modules)
            module.serialize();

        return true;
    }
}
