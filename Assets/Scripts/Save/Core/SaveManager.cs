using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIGGD.Save
{
    /// <summary>
    /// Persistent, singleton save manager. Owns three pipelines — settings, gameplay, and
    /// screenshot — and drives the load-on-scene-enter / save-on-scene-exit lifecycle.
    /// </summary>
    /// <remarks>
    /// <para>Boot &amp; lifecycle:
    /// <list type="number">
    ///   <item><description><see cref="SaveBootstrap"/> spawns the instance before the first scene load (via <see cref="RuntimeInitializeOnLoadMethodAttribute"/>), so no scene needs to place this manually. <see cref="Awake"/> flags it <see cref="Object.DontDestroyOnLoad(Object)"/> and subscribes to <see cref="SceneManager.sceneLoaded"/>.</description></item>
    ///   <item><description><see cref="SaveBootstrap"/> then calls <see cref="Register"/> for every <see cref="ISaveModule"/> and <see cref="LoadSettings"/> to hydrate + push settings into their live runtimes (FMOD volumes, input rebinds, etc.).</description></item>
    ///   <item><description>On every scene load the manager locates the scene's <see cref="SceneSaveConfig"/>, hydrates enabled gameplay modules from disk, auto-applies any that implement <see cref="IAutoApplyOnLoad"/>, then fires <see cref="GameplayDataReady"/>. Scene singletons pull their own state via <see cref="Apply{T}"/> or <see cref="WhenGameplayReady"/> from <c>Start</c> — this is the intended way to avoid race conditions with per-scene <c>Awake</c>/<c>Start</c> ordering.</description></item>
    ///   <item><description>Scene transitions call <see cref="SaveGameplay"/> with <see cref="SaveTrigger.SceneExit"/>. This bypasses <c>GameStateManager</c> so the outgoing scene's state is on disk before the fade — a scene exit is always considered a legitimate save point.</description></item>
    ///   <item><description>App quit / pause calls <see cref="SaveGameplay"/> with <see cref="SaveTrigger.ApplicationExit"/>. This one respects <c>GameStateManager.canSaveGame()</c>, so a player who quits while pursued loses progress by design.</description></item>
    /// </list>
    /// </para>
    /// <para>Threading: everything on this class assumes the Unity main thread. Save-file I/O
    /// is synchronous today (see <see cref="SaveFileIO"/>); if that ever moves to a worker thread,
    /// this class needs a job queue.</para>
    /// </remarks>
    [DefaultExecutionOrder(-1050)]
    [DisallowMultipleComponent]
    public class SaveManager : MonoBehaviour
    {
        // ---------------------------------------------------------------------
        // Singleton
        // ---------------------------------------------------------------------

        private static SaveManager _instance;

        /// <summary>The persistent instance. Populated at boot by <see cref="SaveBootstrap"/>.</summary>
        public static SaveManager Instance => _instance;

        // ---------------------------------------------------------------------
        // Registration
        // ---------------------------------------------------------------------

        private readonly List<ISaveModule> _all = new();
        private readonly Dictionary<string, ISaveModule> _byKey = new();
        private readonly Dictionary<Type, ISaveModule> _byType = new();

        /// <summary>All registered modules, in registration order. Read-only view for diagnostics.</summary>
        public IReadOnlyList<ISaveModule> Modules => _all;

        /// <summary>
        /// Register a module with the manager. Must be called before <see cref="LoadSettings"/>
        /// or the first scene load. Registration is idempotent for the same instance.
        /// </summary>
        public void Register(ISaveModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (string.IsNullOrEmpty(module.Key)) throw new ArgumentException("ISaveModule.Key must be non-empty.", nameof(module));

            if (_byKey.TryGetValue(module.Key, out var existing))
            {
                if (ReferenceEquals(existing, module)) return;
                throw new InvalidOperationException(
                    $"SaveManager: a different module is already registered under key '{module.Key}' ({existing.GetType().Name}).");
            }

            _all.Add(module);
            _byKey.Add(module.Key, module);
            _byType[module.GetType()] = module;
        }

        /// <summary>Get a registered module by its concrete type, or <c>null</c> if none is registered.</summary>
        public T Get<T>() where T : class, ISaveModule
        {
            return _byType.TryGetValue(typeof(T), out var m) ? (T)m : null;
        }

        /// <summary>Get a registered module by key, or <c>null</c>.</summary>
        public ISaveModule Get(string key)
        {
            return key != null && _byKey.TryGetValue(key, out var m) ? m : null;
        }

        // ---------------------------------------------------------------------
        // Scene-level configuration & readiness
        // ---------------------------------------------------------------------

        /// <summary>The active scene's <see cref="SceneSaveConfig"/>, if any. <c>null</c> in menus / boot scenes.</summary>
        public SceneSaveConfig ActiveSceneConfig { get; private set; }

        /// <summary>True once gameplay modules have been deserialized for the currently active scene.</summary>
        public bool HasGameplayDataReady { get; private set; }

        /// <summary>Fired after gameplay modules have been deserialized for the newly loaded scene.</summary>
        public event Action GameplayDataReady;

        /// <summary>
        /// One-shot convenience: invoke <paramref name="callback"/> immediately if gameplay data
        /// is already available for the current scene, otherwise the next time it becomes available.
        /// Safe to call from a scene singleton's <c>Awake</c>/<c>Start</c>.
        /// </summary>
        public void WhenGameplayReady(Action callback)
        {
            if (callback == null) return;
            if (HasGameplayDataReady) { callback(); return; }

            Action handler = null;
            handler = () =>
            {
                GameplayDataReady -= handler;
                callback();
            };
            GameplayDataReady += handler;
        }

        // ---------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------

        private bool _handledInitialScene;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.Log($"SaveManager: destroying duplicate on '{gameObject.name}'.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            // SceneManager.sceneLoaded doesn't fire for the scene that was already active when this
            // GameObject was spawned by SaveBootstrap. Handle it manually if OnSceneLoaded hasn't.
            if (!_handledInitialScene)
            {
                HandleSceneLoaded(SceneManager.GetActiveScene());
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            SaveSettings();
            SaveGameplay(SaveTrigger.ApplicationExit);
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused) return;
            SaveSettings();
            SaveGameplay(SaveTrigger.ApplicationExit);
        }

        // ---------------------------------------------------------------------
        // Scene load pipeline
        // ---------------------------------------------------------------------

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _handledInitialScene = true;
            HandleSceneLoaded(scene);
        }

        private void HandleSceneLoaded(Scene scene)
        {
            HasGameplayDataReady = false;
            ActiveSceneConfig = FindSceneConfig(scene);

            LoadGameplay();

            HasGameplayDataReady = true;
            try
            {
                GameplayDataReady?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static SceneSaveConfig FindSceneConfig(Scene scene)
        {
            if (!scene.IsValid()) return null;

            foreach (var root in scene.GetRootGameObjects())
            {
                var config = root.GetComponentInChildren<SceneSaveConfig>(includeInactive: true);
                if (config != null) return config;
            }
            return null;
        }

        /// <summary>Whether a module is enabled by the currently active scene config.</summary>
        public bool IsModuleEnabledInActiveScene(ISaveModule module)
        {
            if (module == null) return false;
            // Only gameplay modules honour per-scene overrides — settings and screenshots are global.
            if (module.Scope != SaveScope.Gameplay) return true;
            if (ActiveSceneConfig == null) return true;
            return ActiveSceneConfig.IsModuleEnabled(module.Key, defaultEnabled: true);
        }

        // ---------------------------------------------------------------------
        // Load / Save / Apply
        // ---------------------------------------------------------------------

        /// <summary>
        /// Hydrate every <see cref="SaveScope.Settings"/> module from disk and immediately apply
        /// it. Called once during boot; safe to call again from tools/tests.
        /// </summary>
        public void LoadSettings()
        {
            for (int i = 0; i < _all.Count; i++)
            {
                var m = _all[i];
                if (m.Scope != SaveScope.Settings) continue;
                TryDeserialize(m);
                // Settings own their live runtime (FMOD VCA volume, InputAction rebinds, ...) so
                // we push immediately rather than waiting for a scene singleton to pull.
                TryApply(m);
            }
        }

        /// <summary>
        /// Hydrate every enabled gameplay module from disk into its POCO. Called once per scene
        /// load, before <see cref="GameplayDataReady"/> fires.
        /// </summary>
        private void LoadGameplay()
        {
            for (int i = 0; i < _all.Count; i++)
            {
                var m = _all[i];
                if (m.Scope != SaveScope.Gameplay) continue;
                if (!IsModuleEnabledInActiveScene(m)) continue;
                TryDeserialize(m);

                // IAutoApplyOnLoad opts a module into being applied here, before any scene
                // singleton runs. Only safe when Apply() targets something guaranteed to exist
                // at this point (a LazySingleton, a static service, ...). Anything that reads
                // from a scene MonoBehaviour must NOT implement IAutoApplyOnLoad; those pull
                // themselves via Apply<T>() / WhenGameplayReady() from Start().
                if (m is IAutoApplyOnLoad) TryApply(m);
            }
        }

        private static void TryDeserialize(ISaveModule module)
        {
            try
            {
                if (SaveFileIO.Read(module.Scope, module.Key, out byte[] payload, out int version))
                {
                    module.Deserialize(payload, version);
                }
                else
                {
                    // No file on disk: hand the module an empty payload so it resets its POCO
                    // and clears IsLoaded. This is what prevents stale in-memory data from a
                    // previous scene (or from a save that was just wiped via ResetGameplay) from
                    // being re-applied over the freshly authored scene state.
                    module.Deserialize(Array.Empty<byte>(), module.Version);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: deserialize failed for '{module.Key}': {e}");
            }
        }

        private static void TryApply(ISaveModule module)
        {
            try
            {
                module.Apply();
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Apply failed for '{module.Key}': {e}");
            }
        }

        /// <summary>
        /// Push the in-memory POCO of the module of type <typeparamref name="T"/> onto the live scene.
        /// Returns <c>false</c> if the module is not registered or is disabled in the current scene.
        /// </summary>
        public bool Apply<T>() where T : class, ISaveModule
        {
            var module = Get<T>();
            return ApplyInternal(module);
        }

        /// <summary>Push the in-memory POCO of the module with the given key onto the live scene.</summary>
        public bool Apply(string key) => ApplyInternal(Get(key));

        private bool ApplyInternal(ISaveModule module)
        {
            if (module == null) return false;
            if (!IsModuleEnabledInActiveScene(module)) return false;

            try
            {
                module.Apply();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Apply failed for '{module.Key}': {e}");
                return false;
            }
        }

        /// <summary>
        /// Persist every <see cref="SaveScope.Settings"/> module. Never gated; safe to call any time.
        /// </summary>
        public void SaveSettings()
        {
            for (int i = 0; i < _all.Count; i++)
            {
                var m = _all[i];
                if (m.Scope != SaveScope.Settings) continue;
                TryCaptureAndSerialize(m);
            }
        }

        /// <summary>
        /// Persist every enabled <see cref="SaveScope.Gameplay"/> module. Gating rules:
        /// <list type="bullet">
        ///   <item><description><see cref="SaveTrigger.SceneExit"/> — forced. Ignores <c>GameStateManager</c>.</description></item>
        ///   <item><description>All other triggers — respect <c>GameStateManager.canSaveGame()</c> (e.g. pursued player can't save).</description></item>
        /// </list>
        /// Additionally short-circuits when no gameplay scene is loaded (no <c>PlayerID.Instance</c>).
        /// Without this, an <c>OnApplicationQuit</c> fired from the main menu would happily serialize
        /// every module's default-constructed POCO on top of a valid saved run.
        /// </summary>
        /// <returns><c>true</c> if the save actually ran to completion.</returns>
        public bool SaveGameplay(SaveTrigger trigger)
        {
            if (PlayerID.Instance == null)
            {
                Debug.Log($"SaveManager: gameplay save ({trigger}) skipped — no gameplay scene is active.");
                return false;
            }

            bool forced = trigger == SaveTrigger.SceneExit;
            if (!forced && !CanSaveGameplay())
            {
                Debug.Log($"SaveManager: gameplay save ({trigger}) skipped — GameStateManager blocks it.");
                return false;
            }

            for (int i = 0; i < _all.Count; i++)
            {
                var m = _all[i];
                if (m.Scope != SaveScope.Gameplay) continue;
                if (!IsModuleEnabledInActiveScene(m)) continue;
                TryCaptureAndSerialize(m);
            }

            if (trigger == SaveTrigger.ManualWithScreenshot)
            {
                SaveScreenshot();
            }
            return true;
        }

        /// <summary>Persist every <see cref="SaveScope.Screenshot"/> module. Used by manual save UI.</summary>
        public void SaveScreenshot()
        {
            for (int i = 0; i < _all.Count; i++)
            {
                var m = _all[i];
                if (m.Scope != SaveScope.Screenshot) continue;
                TryCaptureAndSerialize(m);
            }
        }

        private static void TryCaptureAndSerialize(ISaveModule module)
        {
            try
            {
                module.Capture();
                byte[] payload = module.Serialize();
                if (payload == null)
                {
                    Debug.LogWarning($"SaveManager: module '{module.Key}' returned null payload — skipped write.");
                    return;
                }
                SaveFileIO.Write(module.Scope, module.Key, module.Version, payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: capture/serialize failed for '{module.Key}': {e}");
            }
        }

        /// <summary>
        /// Wraps <c>GameStateManager.canSaveGame()</c>. Missing manager (edit-mode, early boot,
        /// menu-only scenes) is treated as "allowed" so we don't NRE.
        /// </summary>
        private static bool CanSaveGameplay()
        {
            var gsm = GameStateManager.Instance;
            return gsm == null || gsm.canSaveGame();
        }

        // ---------------------------------------------------------------------
        // Reset (new-game)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Wipe every gameplay file from disk and reset each gameplay module's in-memory POCO to
        /// its default state. Call from the "start new game" / prologue flow. Does NOT touch
        /// <see cref="SaveScope.Settings"/> modules — the player keeps their volume / rebinds.
        /// </summary>
        public void ResetGameplay()
        {
            SaveFileIO.ClearGameplay();

            for (int i = 0; i < _all.Count; i++)
            {
                var m = _all[i];
                if (m.Scope != SaveScope.Gameplay) continue;

                // Feeding Deserialize an empty payload is the module's own reset hook — it
                // rebuilds defaults and clears IsLoaded. See ISaveModule.Deserialize.
                try
                {
                    m.Deserialize(Array.Empty<byte>(), m.Version);
                }
                catch (Exception e)
                {
                    Debug.LogError($"SaveManager: reset (deserialize-defaults) failed for '{m.Key}': {e}");
                }
            }
        }
    }
}
