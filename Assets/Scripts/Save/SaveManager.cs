using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public InventoryDataSaveModule inventoryModule = null;
    public bool saveInventory = true;
    
    public PlayerDataSaveModule playerModule = null;
    public bool savePlayer = true;

    public ScreenshotSaveModule screenshotModule = null;
    public bool saveScreenshot = true;

    public QuestDataSaveModule questModule = null;
    public bool saveQuests = true;

    public GameProgressDataSaveModule gameProgressModule = null;
    public bool saveGameProgress = true;

    private ISaveModule[] modules;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        if (saveInventory) inventoryModule = new InventoryDataSaveModule();
        if (savePlayer) playerModule = new PlayerDataSaveModule();
        if (saveScreenshot) screenshotModule = new ScreenshotSaveModule();
        if (saveQuests) questModule = new QuestDataSaveModule();
        if (saveGameProgress) gameProgressModule = new GameProgressDataSaveModule();

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
        {
            module?.deserialize();
        }

        return true;
    }

    public bool Save()
    {
        foreach (var module in modules)
        {
            module?.serialize();
        }
        return true;
    }
}
