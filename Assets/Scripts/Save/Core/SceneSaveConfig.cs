using UnityEngine;

namespace SIGGD.Save
{
    /// <summary>
    /// Per-scene save configuration. Drop this on a root GameObject in any scene that needs
    /// non-default save behaviour (e.g. the tutorial scene disables inventory + quests).
    /// </summary>
    /// <remarks>
    /// <see cref="SaveManager"/> looks up the first <see cref="SceneSaveConfig"/> found in the
    /// newly loaded scene and consults <see cref="profile"/> before running Capture/Apply on
    /// each gameplay module. If no config exists in the scene, all gameplay modules run.
    /// </remarks>
    [DisallowMultipleComponent]
    public class SceneSaveConfig : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Optional per-scene override profile. Leave empty to save all gameplay modules.")]
        private SaveOverrideProfile profile;

        /// <summary>The override profile, or <c>null</c> if this scene uses defaults.</summary>
        public SaveOverrideProfile Profile => profile;

        /// <summary>
        /// Convenience shim: returns whether the given module key is enabled in this scene,
        /// defaulting to <paramref name="defaultEnabled"/> when there is no profile or the
        /// profile does not mention the module.
        /// </summary>
        public bool IsModuleEnabled(string moduleKey, bool defaultEnabled = true)
        {
            if (profile == null) return defaultEnabled;
            return profile.IsEnabled(moduleKey, defaultEnabled);
        }
    }
}
