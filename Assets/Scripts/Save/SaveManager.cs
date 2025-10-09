using System.Collections.Generic;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public List<ISaveModule> modules = new List<ISaveModule>();

    new private void Awake()
    {
        PlayerDataSaveModule.player = gameObject;
        Load();
    }

    private void OnEnable()
    {
        Application.wantsToQuit += Save;
    }

    public bool Load()
    {
        foreach (ISaveModule i in modules)
        {
            i.deserialize();
        }

        return true;
    }

    public bool Save()
    {
        foreach(ISaveModule i in modules)
        {
            i.serialize();
        }

        return true;
    }
}
