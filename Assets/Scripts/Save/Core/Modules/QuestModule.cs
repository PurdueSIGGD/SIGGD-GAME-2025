using System;
using Sirenix.Serialization;
using UnityEngine;

namespace SIGGD.Save.Modules
{
    /// <summary>
    /// Persists per-run quest objective progress via GUID-keyed <see cref="QuestSaveData"/>.
    /// Ported from <c>QuestDataSaveModule</c>.
    /// </summary>
    /// <remarks>
    /// The target is <c>QuestManager.Instance</c>, which is a <c>LazySingleton</c> (always
    /// available), so this module is marked <see cref="IAutoApplyOnLoad"/> and
    /// <see cref="SaveManager"/> Applies it automatically after each Deserialize — no scene
    /// singleton needs to wire it up.
    /// </remarks>
    public class QuestModule : ISaveModule, IAutoApplyOnLoad
    {
        public string Key => "quests";
        public SaveScope Scope => SaveScope.Gameplay;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        /// <summary>The in-memory POCO. Always non-null after construction.</summary>
        public QuestSaveData Data { get; private set; } = new();

        public void Capture()
        {
            var qm = QuestManager.Instance;
            if (qm == null)
            {
                Debug.LogWarning("QuestModule: QuestManager.Instance is null — capture skipped.");
                return;
            }
            var registryHub = RegistryHub.Instance;
            if (registryHub == null)
            {
                Debug.LogWarning("QuestModule: RegistryHub.Instance is null — capture skipped.");
                return;
            }

            var instances = qm.GetQuestInstances();
            Data = QuestSaveData.FromQuestInstances(instances, registryHub);
        }

        public void Apply()
        {
            if (!IsLoaded)
            {
                // No real save on disk — leave QuestManager's authored/registered state intact.
                return;
            }

            var registryHub = RegistryHub.Instance;
            if (registryHub == null)
            {
                Debug.LogWarning("QuestModule: RegistryHub.Instance is null — apply skipped.");
                return;
            }

            var qm = QuestManager.Instance;
            if (qm == null)
            {
                Debug.LogWarning("QuestModule: QuestManager.Instance is null — apply skipped.");
                return;
            }

            var runtime = Data.ToQuestInstances(registryHub);
            qm.LoadSavedData(runtime);
        }

        public byte[] Serialize()
        {
            var ctx = new SerializationContext { IndexReferenceResolver = new UnityReferenceResolver() };
            return SerializationUtility.SerializeValue(Data, DataFormat.Binary, ctx);
        }

        public void Deserialize(byte[] bytes, int version)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Data = new QuestSaveData();
                IsLoaded = false;
                return;
            }

            try
            {
                var ctx = new DeserializationContext { IndexReferenceResolver = new UnityReferenceResolver() };
                Data = SerializationUtility.DeserializeValue<QuestSaveData>(bytes, DataFormat.Binary, ctx) ?? new QuestSaveData();
                IsLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"QuestModule: deserialize failed (v{version}), resetting to defaults: {e}");
                Data = new QuestSaveData();
                IsLoaded = false;
            }
        }
    }
}
