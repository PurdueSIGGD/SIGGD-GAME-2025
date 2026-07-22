using UnityEngine;

namespace SIGGD.Save.Modules
{
    /// <summary>
    /// Encodes the current <see cref="Camera.main"/> view as a PNG. Ported from
    /// <c>ScreenshotSaveModule</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="SaveScope.Screenshot"/> — only serialised on explicit "save game" requests
    /// (<see cref="SaveTrigger.ManualWithScreenshot"/>). Never deserialised or applied by the
    /// pipeline; the main menu reads the file directly for preview purposes via
    /// <see cref="ScreenshotFilePath"/>.
    /// </remarks>
    public class ScreenshotModule : ISaveModule
    {
        public string Key => "screenshot";
        public SaveScope Scope => SaveScope.Screenshot;
        public int Version => 1;
        public bool IsLoaded { get; private set; }

        /// <summary>Absolute path of the last-saved screenshot on disk.</summary>
        public string ScreenshotFilePath => SaveFileIO.GetPath(Scope, Key);

        private byte[] _lastPng;

        public void Capture()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                _lastPng = null;
                return;
            }

            int width = cam.pixelWidth;
            int height = cam.pixelHeight;

            RenderTexture rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);

            cam.Render();
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = prev;

            _lastPng = tex.EncodeToPNG();

            Object.Destroy(tex);
            Object.Destroy(rt);
        }

        public void Apply()
        {
            // Screenshots are never applied to the scene.
        }

        public byte[] Serialize() => _lastPng;

        public void Deserialize(byte[] bytes, int version)
        {
            if (bytes == null || bytes.Length == 0)
            {
                _lastPng = null;
                IsLoaded = false;
                return;
            }
            _lastPng = bytes;
            IsLoaded = true;
        }
    }
}
