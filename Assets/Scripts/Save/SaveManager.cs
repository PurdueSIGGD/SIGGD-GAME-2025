public class SaveManager : Singleton<SaveManager>
{
    public ISaveModule[] modules = {
        new PlayerDataSaveModule()
    };

    new private void Awake()
    {
        PlayerDataSaveModule.player = gameObject;
        Load();
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
