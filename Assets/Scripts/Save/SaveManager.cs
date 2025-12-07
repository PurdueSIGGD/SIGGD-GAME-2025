using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    InventoryDataSaveModule inventoryModule;
    PlayerDataSaveModule playerModule;
    ScreenshotSaveModule screenshotModule;
    QuestDataSaveModule questModule;

    private ISaveModule[] modules;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        inventoryModule = new InventoryDataSaveModule();
        screenshotModule = new ScreenshotSaveModule();
        playerModule = new PlayerDataSaveModule();
        questModule = new QuestDataSaveModule();

        modules = new ISaveModule[] {inventoryModule, screenshotModule, playerModule, questModule};

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
