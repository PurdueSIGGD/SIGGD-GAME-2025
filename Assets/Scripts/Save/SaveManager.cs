using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    private const float AUTOSAVE_INTERVAL_SECONDS = 300.0f;

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

        // Start Autosaving - every five minutes
        InvokeRepeating(nameof(Save), AUTOSAVE_INTERVAL_SECONDS, AUTOSAVE_INTERVAL_SECONDS);


    }

    private void OnApplicationQuit()
    {
        if (GameStateManager.Instance.canSaveGame())
        {
            Save();
        } else
        {
            Debug.Log("Application closed, but game was not saved as GameStateManager currentState  = " +
                      GameStateManager.Instance.getGameState());
        }
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
        if (!GameStateManager.Instance.canSaveGame())
        {
            return false;
        }

        Debug.Log("SaveManager : Game was saved.");

        foreach (var module in modules)
        {
            module?.serialize();
        }
        return true;
    }

}
