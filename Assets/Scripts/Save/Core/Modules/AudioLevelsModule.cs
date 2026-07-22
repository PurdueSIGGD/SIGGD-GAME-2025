using System;
using System.Collections.Generic;
using FMODUnity;
using Sirenix.Serialization;
using UnityEngine;

namespace SIGGD.Save.Modules
{
    /// <summary>
    /// Persists per-VCA FMOD volumes. Lives in <see cref="SaveScope.Settings"/> — global, not
    /// scoped to a run or scene.
    /// </summary>
    /// <remarks>
    /// <see cref="Apply"/> pushes the stored dictionary onto the live FMOD VCAs;
    /// <see cref="Capture"/> pulls current volumes back. Bump <see cref="Version"/> if you change
    /// <see cref="VcaNames"/> in a way that isn't additive.
    /// </remarks>
    public class AudioLevelsModule : ISaveModule
    {
        private static readonly string[] VcaNames = { "Master", "Music", "Ambience", "SFX", "Voicelines" };

        public string Key => "audio";
        public SaveScope Scope => SaveScope.Settings;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        public AudioLevelsSaveData Data { get; private set; } = new() { audioLevels = new Dictionary<string, float>() };

        public void Capture()
        {
            Data.audioLevels ??= new Dictionary<string, float>();
            foreach (string vcaName in VcaNames)
            {
                if (RuntimeManager.GetVCA("vca:/" + vcaName).getVolume(out float volume) == FMOD.RESULT.OK)
                {
                    Data.audioLevels[vcaName] = volume;
                }
            }
        }

        public void Apply()
        {
            if (Data.audioLevels == null) return;
            foreach (string vcaName in VcaNames)
            {
                if (Data.audioLevels.TryGetValue(vcaName, out float volume))
                {
                    RuntimeManager.GetVCA("vca:/" + vcaName).setVolume(volume);
                }
            }
        }

        public byte[] Serialize() => SerializationUtility.SerializeValue(Data, DataFormat.Binary);

        public void Deserialize(byte[] bytes, int version)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Data = new AudioLevelsSaveData { audioLevels = new Dictionary<string, float>() };
                IsLoaded = false;
                return;
            }
            try
            {
                Data = SerializationUtility.DeserializeValue<AudioLevelsSaveData>(bytes, DataFormat.Binary)
                       ?? new AudioLevelsSaveData { audioLevels = new Dictionary<string, float>() };
                Data.audioLevels ??= new Dictionary<string, float>();
                IsLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AudioLevelsModule: deserialize failed (v{version}), resetting to defaults: {e}");
                Data = new AudioLevelsSaveData { audioLevels = new Dictionary<string, float>() };
                IsLoaded = false;
            }
        }

        /// <summary>Reset every VCA to full volume (1.0), update the live FMOD state, and persist immediately.</summary>
        public void ResetAudioLevels()
        {
            foreach (string vcaName in VcaNames)
            {
                RuntimeManager.GetVCA("vca:/" + vcaName).setVolume(1.0f);
                Data.audioLevels[vcaName] = 1.0f;
            }
            SaveManager.Instance?.SaveSettings();
        }
    }
}
