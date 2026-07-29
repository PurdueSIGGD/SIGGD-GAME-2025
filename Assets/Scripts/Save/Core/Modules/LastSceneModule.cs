using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIGGD.Save.Modules
{
    /// <summary>
    /// Remembers the last <em>gameplay</em> scene the player was in so the main menu's
    /// "Continue" button can jump straight back to it. Replaces the old
    /// <c>SceneDataSaveModule</c> + <c>SceneSaveManager</c> pair.
    /// </summary>
    /// <remarks>
    /// <see cref="Capture"/> only writes when the currently active scene looks like a gameplay
    /// scene (i.e. <c>PlayerID.Instance</c> exists and the scene name is not in the menu
    /// blacklist). This keeps menu-scene saves from clobbering the last real gameplay scene.
    /// </remarks>
    public class LastSceneModule : ISaveModule
    {
        // Scene names that are never remembered as "the last gameplay scene".
        private static readonly HashSet<string> MenuSceneBlacklist = new(StringComparer.Ordinal)
        {
            "GameStart",
            "Main Menu",
            "Credtis",
        };

        public string Key => "scene";
        public SaveScope Scope => SaveScope.Gameplay;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        /// <summary>The in-memory POCO. Always non-null.</summary>
        public SceneSaveData Data { get; private set; } = new();

        /// <summary>Convenience accessor mirroring the old <c>SceneSaveManager.sceneName</c> field.</summary>
        public string SceneName => Data.sceneName ?? string.Empty;

        public void Capture()
        {
            string active = SceneManager.GetActiveScene().name;
            if (PlayerID.Instance == null || MenuSceneBlacklist.Contains(active))
            {
                // Menu / boot scene — do not overwrite the last known gameplay scene.
                return;
            }
            Data.sceneName = active;
        }

        public void Apply()
        {
            // Nothing to sync to the scene; external code reads Data.sceneName directly.
        }

        public byte[] Serialize() => SerializationUtility.SerializeValue(Data, DataFormat.Binary);

        public void Deserialize(byte[] bytes, int version)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Data = new SceneSaveData();
                IsLoaded = false;
                return;
            }
            try
            {
                Data = SerializationUtility.DeserializeValue<SceneSaveData>(bytes, DataFormat.Binary) ?? new SceneSaveData();
                IsLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"LastSceneModule: deserialize failed (v{version}), resetting to defaults: {e}");
                Data = new SceneSaveData();
                IsLoaded = false;
            }
        }
    }
}
