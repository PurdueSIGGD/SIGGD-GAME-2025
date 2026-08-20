using System;
using Sirenix.Serialization;
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
    public class PlayerModule : ISaveModule
    {
        public string Key => "player";
        public SaveScope Scope => SaveScope.Gameplay;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        /// <summary>The in-memory POCO. Always non-null after construction.</summary>
        public PlayerSaveData Data { get; private set; } = new();

        // -------------------------------------------------------------------
        // Convenience accessors for gameplay code
        //
        // These fields are still authored via the module rather than being
        // migrated to live player components (see the migration plan). The
        // properties exist so callers write `player.HasGloves = true` instead
        // of reaching into `Data` directly.
        // -------------------------------------------------------------------

        /// <summary>Whether the player has picked up the climbing gloves upgrade.</summary>
        public bool HasGloves
        {
            get => Data.hasGloves;
            set => Data.hasGloves = value;
        }

        /// <summary>Current slime tier (0 most contaminated, 4 fully treated).</summary>
        public int SlimeLevel
        {
            get => Data.slimeLevel;
            set => Data.slimeLevel = value;
        }

        public void Capture()
        {
            var pid = PlayerID.Instance;
            if (pid == null)
            {
                Debug.Log("PlayerModule: no PlayerID in scene — capture skipped.");
                return;
            }

            var cam = pid.cam;
            var health = pid.playerHealth;
            var hunger = pid.playerHunger;
            var stamina = pid.playerStamina;
            var radiation = pid.playerRadiation;
            var respawn = pid.GetComponent<ManageRespawn>();

            if (cam == null || health == null || hunger == null || stamina == null)
            {
                Debug.LogWarning("PlayerModule: player is missing required components — capture aborted.");
                return;
            }

            // Remember which scene this pose belongs to so Apply can gate on scene identity later.
            Data.sceneName = SceneManager.GetActiveScene().name;

            // If the player is dead, freeze position at the respawn point so a follow-up load doesn't
            // drop them at a dying-frame location.
            Data.Position = pid.IsAlive ? pid.transform.position : (respawn != null ? respawn.respawnPoint : pid.transform.position);
            Data.Rotation = cam.GetRotation();
            Data.curHealth = health.CurrentHealth;
            Data.curHunger = hunger.CurrentHunger;
            Data.curStamina = stamina.CurrentStamina;
            Data.staminaDisabled = stamina.StaminaDisabled;
            Data.RespawnPosition = respawn != null ? respawn.respawnPoint : Data.RespawnPosition;
            if (radiation != null) Data.radiationLevel = radiation.CurrentRadiation;

            // hasGloves and slimeLevel are edited directly on Data by their interaction scripts; no live source to pull from.
        }

        public void Apply()
        {
            if (!IsLoaded)
            {
                // No real save on disk — leave the scene-authored player transform / stats alone.
                return;
            }

            var pid = PlayerID.Instance;
            if (pid == null)
            {
                Debug.Log("PlayerModule: no PlayerID in scene — apply skipped.");
                return;
            }

            var cam = pid.cam;
            var health = pid.playerHealth;
            var hunger = pid.playerHunger;
            var stamina = pid.playerStamina;
            var radiation = pid.playerRadiation;
            var respawn = pid.GetComponent<ManageRespawn>();

            // Stats persist across scenes.
            if (health != null) health.CurrentHealth = Data.curHealth;
            if (hunger != null) hunger.CurrentHunger = Data.curHunger;
            if (stamina != null)
            {
                stamina.CurrentStamina = Data.curStamina;
                stamina.StaminaDisabled = Data.staminaDisabled;
            }
            if (radiation != null) radiation.CurrentRadiation = Data.radiationLevel;

            // Position / rotation / respawn are scene-scoped. Only push them onto the live player
            // when the save was taken in the currently active scene — otherwise let the scene
            // itself (or SceneFader.TransitionRoutine) dictate where the player starts.
            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(Data.sceneName) && Data.sceneName == currentScene)
            {
                pid.transform.position = Data.Position;
                cam?.SetRotation(Data.Rotation);
                if (respawn != null) respawn.respawnPoint = Data.RespawnPosition;
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
                Data = new PlayerSaveData();
                IsLoaded = false;
                return;
            }
            try
            {
                Data = SerializationUtility.DeserializeValue<PlayerSaveData>(bytes, DataFormat.Binary) ?? new PlayerSaveData();
                IsLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerModule: deserialize failed (v{version}), resetting to defaults: {e}");
                Data = new PlayerSaveData();
                IsLoaded = false;
            }
        }
    }
}
