using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SIGGD.Save.Modules
{
    /// <summary>
    /// Persists Input System rebind overrides for the <c>PlayerInputActions</c> asset.
    /// </summary>
    /// <remarks>
    /// The stored payload is the raw JSON produced by <see cref="InputActionAsset.SaveBindingOverridesAsJson"/>.
    /// <see cref="Apply"/> writes overrides onto both the asset and the live <c>PlayerInput</c>
    /// instance so already-enabled actions pick up the new bindings without a domain reload.
    /// </remarks>
    public class InputOverrideModule : ISaveModule
    {
        public string Key => "input";
        public SaveScope Scope => SaveScope.Settings;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        /// <summary>The current bindings-override JSON string. Empty when no overrides have been captured.</summary>
        public string OverridesJson { get; private set; } = string.Empty;

        private InputActionAsset _asset;

        private InputActionAsset FindAsset()
        {
            if (_asset != null) return _asset;
            foreach (var asset in Resources.FindObjectsOfTypeAll<InputActionAsset>())
            {
                if (asset.name == "PlayerInputActions")
                {
                    _asset = asset;
                    break;
                }
            }
            return _asset;
        }

        public void Capture()
        {
            var asset = FindAsset();
            if (asset == null)
            {
                Debug.LogWarning("InputOverrideModule: PlayerInputActions asset not found; capture skipped.");
                return;
            }
            OverridesJson = asset.SaveBindingOverridesAsJson() ?? string.Empty;
        }

        public void Apply()
        {
            var asset = FindAsset();
            if (asset == null)
            {
                Debug.LogWarning("InputOverrideModule: PlayerInputActions asset not found; apply skipped.");
                return;
            }
            asset.LoadBindingOverridesFromJson(OverridesJson ?? string.Empty);

            if (PlayerInput.Instance != null)
            {
                PlayerInput.Instance.LoadBindingOverrides(OverridesJson ?? string.Empty);
            }
        }

        public byte[] Serialize() => Encoding.UTF8.GetBytes(OverridesJson ?? string.Empty);

        public void Deserialize(byte[] bytes, int version)
        {
            if (bytes == null || bytes.Length == 0)
            {
                OverridesJson = string.Empty;
                IsLoaded = false;
                return;
            }
            OverridesJson = Encoding.UTF8.GetString(bytes);
            IsLoaded = true;
        }

        /// <summary>Clear all binding overrides, push the reset to the live asset, and persist.</summary>
        public void ResetPlayerInputs()
        {
            OverridesJson = "{}";
            Apply();
            SaveManager.Instance?.SaveSettings();
        }
    }
}
