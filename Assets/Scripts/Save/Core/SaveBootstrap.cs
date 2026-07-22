using SIGGD.Save.Modules;
using UnityEngine;

namespace SIGGD.Save
{
    /// <summary>
    /// Boot-time initializer for the save system. Spawns the persistent <see cref="SaveManager"/>,
    /// registers every <see cref="ISaveModule"/>, and hydrates the settings pipeline before the
    /// first scene loads.
    /// </summary>
    /// <remarks>
    /// This runs via <see cref="RuntimeInitializeOnLoadMethodAttribute"/> at
    /// <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/>, so no scene needs to place a
    /// <see cref="SaveManager"/> instance manually — including the <c>GameStart</c> bootstrapper.
    /// Register new modules here; the order controls Capture/Serialize order but modules should
    /// not depend on each other's capture/apply timing.
    /// </remarks>
    public static class SaveBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (SaveManager.Instance != null) return;

            GameObject go = new GameObject("SaveManager");
            SaveManager manager = go.AddComponent<SaveManager>();

            // Settings modules: loaded once here, applied immediately, saved on every user change.
            manager.Register(new GameProgressModule());
            manager.Register(new AudioLevelsModule());
            manager.Register(new InputOverrideModule());

            // Gameplay modules: loaded on every scene enter. Scene singletons pull their state
            // from Start(); modules that implement IAutoApplyOnLoad are pushed automatically.
            manager.Register(new PlayerModule());
            manager.Register(new InventoryModule());
            manager.Register(new QuestModule());
            manager.Register(new GraveModule());
            manager.Register(new LastSceneModule());

            // Screenshot module: never auto-loaded, written on explicit "save game" requests.
            manager.Register(new ScreenshotModule());

            manager.LoadSettings();
        }
    }
}
