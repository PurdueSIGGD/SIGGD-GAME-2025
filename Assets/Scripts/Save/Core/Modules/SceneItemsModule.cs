using System;
using Sirenix.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIGGD.Save.Modules
{
    /// <summary>
    /// Player state: pose, health, hunger, stamina, radiation, respawn point, and the
    /// player-owned progression flags (<c>hasGloves</c>, <c>slimeLevel</c>).
    /// </summary>
    /// <remarks>
    /// <para>Both <see cref="Capture"/> and <see cref="Apply"/> require <c>PlayerID.Instance</c>.
    /// If it's missing (menu scenes, edit-mode), they log and no-op — so an application-exit
    /// save fired from the main menu is a safe skip.</para>
    ///
    /// <para>Position, rotation and respawn point are <b>scene-scoped</b>: <see cref="Capture"/>
    /// stamps the active scene name, and <see cref="Apply"/> only pushes those fields when the
    /// saved scene matches the currently active scene. Otherwise the scene's own spawn logic
    /// (e.g. <c>SceneFader.TransitionRoutine</c>) is left to place the player. Stats and
    /// progression flags are scene-agnostic and always applied.</para>
    /// </remarks>
    public class SceneItemsModule : ISaveModule
    {
        public string Key => "sceneItems";
        public SaveScope Scope => SaveScope.Gameplay;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        /// <summary>The in-memory POCO. Always non-null after construction.</summary>
        public SceneItemsSaveData Data { get; private set; } = new();

        // -------------------------------------------------------------------
        // Convenience accessors for gameplay code
        //
        // These fields are still authored via the module rather than being
        // migrated to live player components (see the migration plan). The
        // properties exist so callers write `player.HasGloves = true` instead
        // of reaching into `Data` directly.
        // -------------------------------------------------------------------

        public bool VillageDestroyed
        {
            get => Data.villageDestroyed;
            set => Data.villageDestroyed = value;
        }

        public void Capture()
        {
            Data.sceneName = SceneManager.GetActiveScene().name;
        }

        public void Apply()
        {
            if (!IsLoaded)
            {
                // No real save on disk — leave the scene-authored player transform / stats alone.
                return;
            }

            int? slimeLevel = SaveManager.Instance?.Get<PlayerModule>()?.SlimeLevel;

            Debug.Log("Slime:" + slimeLevel);
            
            for (int i = 0; i < slimeLevel; i++)
            {
                if (MusicBoxIndex.musicBoxes[i] != null)
                {
                    Debug.Log("Try Used index" + i);
                    MusicBoxIndex.musicBoxes[i].Used();
                }
            }
            
            // Position / rotation / respawn are scene-scoped. Only push them onto the live player
            // when the save was taken in the currently active scene — otherwise let the scene
            // itself (or SceneFader.TransitionRoutine) dictate where the player starts.
            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(Data.sceneName) && Data.sceneName == currentScene)
            {
                if (Data.villageDestroyed && VillageDestructionExternalEvent.Instance != null)
                {
                    VillageDestructionExternalEvent.Instance.AlreadyDestroy();
                }
            }
            else
            {
                Debug.Log($"PlayerModule: saved pose is for scene '{Data.sceneName}', current is '{currentScene}' — pose/respawn skipped.");
            }
        }

        public byte[] Serialize() => SerializationUtility.SerializeValue(Data, DataFormat.Binary);

        public void Deserialize(byte[] bytes, int version)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Data = new SceneItemsSaveData();
                IsLoaded = false;
                return;
            }
            try
            {
                Data = SerializationUtility.DeserializeValue<SceneItemsSaveData>(bytes, DataFormat.Binary) ?? new SceneItemsSaveData();
                IsLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerModule: deserialize failed (v{version}), resetting to defaults: {e}");
                Data = new SceneItemsSaveData();
                IsLoaded = false;
            }
        }
    }
}
