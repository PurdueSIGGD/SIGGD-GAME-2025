using System.IO;
using UnityEngine;

public class ScreenshotSaveModule : ISaveModule
{
    public static string savePath = $"{FileManager.savesDirectory}/screenshot.png";

    public static Camera cam;

    public ScreenshotSaveModule()
    {
        Debug.Log("I HAVE BEEN INSTANTIATED.");
    }

    // Returns PNG data of a screenshot from a camera
    private byte[] makeScreenshot(Camera camera)
    {
        int width = camera.pixelWidth, height = camera.pixelHeight;

        // This makes a RenderTexture for the camera render to instead of
        // reading pixels straight from the screen, because the screen may have things
        // such as a HUD that we do not want in our screenshot.
        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        camera.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;

        byte[] pngData = screenshot.EncodeToPNG();
        return pngData;
    }

    public bool deserialize()
    {

        return true;
    }

    public bool serialize()
    {
        Debug.Log("Serializing...");
        Debug.Log(cam);
        FileManager.Instance.WriteFile(savePath, makeScreenshot(cam));

        return true;
    }
}
