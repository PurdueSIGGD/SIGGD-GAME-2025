using System;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIGGD.Save.Modules
{
    /// <summary>
    /// Persists the current grave (if any) — position, rotation, and its stored inventory.
    /// Ported from <c>GraveDataSaveModule</c>.
    /// </summary>
    /// <remarks>
    /// The grave is scene-scoped: <see cref="Capture"/> records the active scene name and
    /// <see cref="Apply"/> only respawns the grave when the saved scene matches the currently
    /// active one — a grave dropped in ShipScene must never appear in NathanA0.
    ///
    /// Fixes a bug in the original module: it called <c>GetCurGrave()</c> but discarded the
    /// return value, meaning the "grave exists" branch was never taken and every save wrote the
    /// "no grave" payload. This port assigns the return value.
    /// </remarks>
    public class GraveModule : ISaveModule
    {
        public string Key => "grave";
        public SaveScope Scope => SaveScope.Gameplay;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        /// <summary>The in-memory POCO. Always non-null after construction.</summary>
        public GraveSaveData Data { get; private set; } = new();

        public void Capture()
        {
            var pid = PlayerID.Instance;
            if (pid == null)
            {
                Debug.Log("GraveModule: no PlayerID in scene — capture skipped.");
                return;
            }
            var respawn = pid.GetComponent<ManageRespawn>();
            if (respawn == null)
            {
                Debug.LogWarning("GraveModule: ManageRespawn is missing on Player — capture skipped.");
                return;
            }

            // Remember which scene this grave lives in so Apply can gate on scene identity later.
            Data.sceneName = SceneManager.GetActiveScene().name;

            GameObject graveObj = respawn.GetCurGrave();
            if (graveObj != null)
            {
                var interact = graveObj.GetComponent<GraveInteract>();
                Data.position = graveObj.transform.position;
                Data.rotation = graveObj.transform.rotation;
                if (interact != null)
                {
                    ItemInfo[] infos = interact.info;
                    Data.names = new string[infos != null ? infos.Length : 0];
                    for (int i = 0; i < Data.names.Length; i++)
                    {
                        Data.names[i] = infos[i] != null ? infos[i].itemName.ToString() : "Empty";
                    }
                    Data.count = interact.count;
                }
                else
                {
                    Data.names = null;
                    Data.count = null;
                }
            }
            else
            {
                Data.position = Vector3.zero;
                Data.rotation = Quaternion.identity;
                Data.names = null;
                Data.count = null;
            }
        }

        public void Apply()
        {
            if (!IsLoaded)
            {
                // No real save on disk — never spawn a grave from empty defaults.
                return;
            }

            if (Data.names == null || Data.count == null)
            {
                // No grave in the loaded save.
                return;
            }

            string currentScene = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(Data.sceneName) || Data.sceneName != currentScene)
            {
                Debug.Log($"GraveModule: saved grave is for scene '{Data.sceneName}', current is '{currentScene}' — apply skipped.");
                return;
            }

            var pid = PlayerID.Instance;
            if (pid == null)
            {
                Debug.Log("GraveModule: no PlayerID in scene — apply skipped.");
                return;
            }
            var respawn = pid.GetComponent<ManageRespawn>();
            if (respawn == null)
            {
                Debug.LogWarning("GraveModule: ManageRespawn is missing on Player — apply skipped.");
                return;
            }

            respawn.CreateGrave(Data.position, Data.rotation, Data.names, Data.count);
        }

        public byte[] Serialize() => SerializationUtility.SerializeValue(Data, DataFormat.Binary);

        public void Deserialize(byte[] bytes, int version)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Data = new GraveSaveData();
                IsLoaded = false;
                return;
            }
            try
            {
                Data = SerializationUtility.DeserializeValue<GraveSaveData>(bytes, DataFormat.Binary) ?? new GraveSaveData();
                IsLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"GraveModule: deserialize failed (v{version}), resetting to defaults: {e}");
                Data = new GraveSaveData();
                IsLoaded = false;
            }
        }
    }
}
