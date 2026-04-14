using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public InputOverrideSaveModule inputOverrideSaveModule = null;
    public bool saveInputOverrides = true;

    public AudioLevelsSaveModule audioLevelsSaveModule = null;
    public bool saveAudioLevels = true;


    private ISaveModule[] modules;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        if (saveInputOverrides) inputOverrideSaveModule = new InputOverrideSaveModule();
        if (saveAudioLevels) audioLevelsSaveModule = new AudioLevelsSaveModule();

        modules = new ISaveModule[] {
            inventoryModule,
            screenshotModule,
            playerModule,
            questModule,
            gameProgressModule,
            mobSceneDataSaveModule,
            graveModule,
            inputOverrideSaveModule,
            audioLevelsSaveModule
        };

        Debug.Log("Loading on start");
        Load();
    }


    // TODO: Need to update to not rely on this
    // Need to update to make sure the SaveModules don't actually have anything to do with setting value in scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Safe to call Load() here because all Awake() methods of scene objects and singletons have run
        Debug.Log($"SaveManager: Loading save after scene '{scene.name}' loaded");
        Load();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("SaveManager OnApplicationQuit " + GameStateManager.Instance.name);
        if (GameStateManager.Instance == null || GameStateManager.Instance.canSaveGame())
        {
            Debug.Log("Saved on application close");
            Save();
        } else
        {
            // TODO: Figure out what to do here -> maybe create a popup modal to say that game cannot be saved before quitting
            Debug.Log("Application closed, but game was not saved as GameStateManager currentState  = " +
                      GameStateManager.Instance.getGameState());
        }
    }

    public bool Load()
    {
        if (modules == null) return false;
        Debug.Log("Loading from save");
        foreach (var module in modules)
        {
            module?.deserialize();
        }

        return true;
    }

    public bool Save()
    {
        // Save if the game state is peaceful

        if ((GameStateManager.Instance != null && !GameStateManager.Instance.canSaveGame()) || modules == null)
        {
            Debug.Log("Couldn't save game");
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
