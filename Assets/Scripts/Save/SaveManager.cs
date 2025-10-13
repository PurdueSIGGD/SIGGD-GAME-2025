using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public ISaveModule[] modules = {
        new PlayerDataSaveModule()
    };

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Save Manager loaded");
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

        return true;
    }

    public bool Save()
    {
        foreach (ISaveModule i in modules)
        {
            i.serialize();
        }

        return true;
    }
}
