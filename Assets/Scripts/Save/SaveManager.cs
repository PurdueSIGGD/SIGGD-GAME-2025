using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public InventoryDataSaveModule inventoryModule;
    public PlayerDataSaveModule playerModule;
    public ScreenshotSaveModule screenshotModule;
    public QuestDataSaveModule questModule;
    public GameProgressDataSaveModule gameProgressModule;

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
        gameProgressModule = new GameProgressDataSaveModule();

        modules = new ISaveModule[] {inventoryModule, screenshotModule, playerModule,
                                     questModule, gameProgressModule};

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
