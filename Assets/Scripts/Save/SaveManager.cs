using System;
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

    public MobSceneDataSaveModule mobSceneDataSaveModule = null;
    public bool saveMobScene = true;
    public GraveDataSaveModule graveModule = null;
    public bool saveGrave = true;

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
        if (saveMobScene) mobSceneDataSaveModule = new MobSceneDataSaveModule(FindFirstObjectByType<MobCensus.MobCensusManager>());
        if (saveGrave) graveModule = new GraveDataSaveModule();

        modules = new ISaveModule[] {
            inventoryModule,
            screenshotModule,
            playerModule,
            questModule,
            gameProgressModule,
            mobSceneDataSaveModule,
            graveModule
        };

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
