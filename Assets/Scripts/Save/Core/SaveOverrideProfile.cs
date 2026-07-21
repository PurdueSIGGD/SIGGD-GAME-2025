using System;
using System.Collections.Generic;
using UnityEngine;

namespace SIGGD.Save
{
    /// <summary>
    /// Editor-authored asset that toggles individual save modules on or off for a specific scene
    /// (e.g. the tutorial scene may want to skip saving inventory and quests).
    /// </summary>
    /// <remarks>
    /// Referenced by <see cref="SceneSaveConfig"/> placed in each scene's root. Any module whose
    /// <see cref="ISaveModule.Key"/> does not appear in <see cref="entries"/> keeps its default
    /// (enabled) state. Only <see cref="SaveScope.Gameplay"/> modules are affected — settings
    /// are always saved.
    /// </remarks>
    [CreateAssetMenu(menuName = "SIGGD/Save/Override Profile", fileName = "SaveOverrideProfile")]
    public class SaveOverrideProfile : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [Tooltip("Must match ISaveModule.Key exactly (e.g. \"player\", \"inventory\", \"quests\", \"grave\", \"scene\").")]
            public string moduleKey;

            [Tooltip("If false, this module is skipped for Capture() and Apply() while this scene is active.")]
            public bool enabled;
        }

        [SerializeField]
        [Tooltip("Per-module overrides. Modules not listed use their default (enabled).")]
        private List<Entry> entries = new();

        /// <summary>
        /// Returns whether the given module is enabled in this profile, defaulting to
        /// <paramref name="defaultEnabled"/> when the profile doesn't mention the module.
        /// </summary>
        public bool IsEnabled(string moduleKey, bool defaultEnabled = true)
        {
            if (string.IsNullOrEmpty(moduleKey)) return defaultEnabled;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].moduleKey == moduleKey) return entries[i].enabled;
            }
            return defaultEnabled;
        }
    }
}
