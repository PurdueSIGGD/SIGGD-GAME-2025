using System;
using Sirenix.Serialization;
using UnityEngine;

namespace SIGGD.Save.Modules
{
    /// <summary>
    /// Persists global, run-agnostic progress flags (currently: prologue completion). Read/write
    /// <see cref="HasCompletedPrologue"/> directly — the module has no live scene target.
    /// </summary>
    /// <remarks>
    /// Lives in <see cref="SaveScope.Settings"/> because these flags survive across new-game
    /// resets and are unaffected by per-scene overrides. Both <see cref="Capture"/> and
    /// <see cref="Apply"/> are intentional no-ops.
    /// </remarks>
    public class GameProgressModule : ISaveModule
    {
        [Serializable]
        public class Data
        {
            public bool hasCompletedPrologue;
        }

        public string Key => "progress";
        public SaveScope Scope => SaveScope.Settings;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        /// <summary>The in-memory POCO. Always non-null after construction.</summary>
        public Data State { get; private set; } = new();

        public bool HasCompletedPrologue
        {
            get => State.hasCompletedPrologue;
            set => State.hasCompletedPrologue = value;
        }

        /// <summary>Flags the prologue as completed. Caller is responsible for triggering a save.</summary>
        public void CompletePrologue()
        {
            State.hasCompletedPrologue = true;
        }

        public void Capture() { /* no live scene target; State is edited directly by callers */ }

        public void Apply() { /* no live scene target; consumers read State directly */ }

        public byte[] Serialize()
        {
            return SerializationUtility.SerializeValue(State, DataFormat.JSON);
        }

        public void Deserialize(byte[] bytes, int version)
        {
            if (bytes == null || bytes.Length == 0)
            {
                State = new Data();
                IsLoaded = false;
                return;
            }

            try
            {
                State = SerializationUtility.DeserializeValue<Data>(bytes, DataFormat.JSON) ?? new Data();
                IsLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"GameProgressModule: deserialize failed (version {version}), resetting to defaults: {e}");
                State = new Data();
                IsLoaded = false;
            }
        }
    }
}
