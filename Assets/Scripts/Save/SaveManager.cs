using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public ISaveModule[] modules = {
        new PlayerDataSaveModule(),
        new ScreenshotSaveModule()
    };

    protected override void Awake()
    {
        base.Awake();
        Load();
    }

    void Start()
    {
        if (PlayerID.Instance != null)
        {
            PlayerDataSaveModule.player = PlayerID.Instance.gameObject;
            ScreenshotSaveModule.cam    = PlayerID.Instance.cam;
        }
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
