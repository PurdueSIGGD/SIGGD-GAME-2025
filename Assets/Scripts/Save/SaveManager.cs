using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public ISaveModule[] modules = {
        new PlayerDataSaveModule()
    };
    public List<ISaveModule> modules;

    protected override void Awake()
    {
        base.Awake();
        Load();
    }

    void Start()
    {
        PlayerDataSaveModule.player = PlayerID.Instance.gameObject;
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public bool Load()
    {
        foreach (ISaveModule i in modules)
        {
            i.deserialize();
        }
        modules = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                  .OfType<ISaveModule>()
                  .ToList();
        foreach (var module in modules)
            module.deserialize();

        return true;
    }

    public bool Save()
    {
        foreach (ISaveModule i in modules)
        {
            i.serialize();
        }
        modules = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                  .OfType<ISaveModule>()
                  .ToList();
        foreach (var module in modules)
            module.serialize();

        return true;
    }
}
