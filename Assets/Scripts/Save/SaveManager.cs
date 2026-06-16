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
        return Save(ignoreGameStateRestriction: false);
    }

    public bool SaveForSceneTransition()
    {
        return Save(ignoreGameStateRestriction: true);
    }

    public bool Save(bool ignoreGameStateRestriction)
    {
        if (modules == null)
        {
            Debug.LogWarning("SaveManager: save skipped because save modules are not initialized yet.");
            return false;
        }

        bool canSaveInCurrentState = GameStateManager.Instance == null || GameStateManager.Instance.canSaveGame();
        if (!ignoreGameStateRestriction && !canSaveInCurrentState)
        {
            Debug.LogWarning(
                $"SaveManager: save blocked by game state '{GameStateManager.Instance.getGameState()}'.");
            return false;
        }

        if (ignoreGameStateRestriction && !canSaveInCurrentState)
        {
            Debug.Log(
                $"SaveManager: forcing transition save while game state is '{GameStateManager.Instance.getGameState()}'.");
        }

        Debug.Log("SaveManager: game was saved.");

        foreach (var module in modules)
        {
            module?.serialize();
        }
        if (SceneSaveManager.Instance != null) SceneSaveManager.Instance.Save();
        return true;
    }
}
