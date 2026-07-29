using System.IO;
using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;
using UnityEngine.UI;

public class Screenshot : MonoBehaviour
{
    Image image;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<Image>();

        string path = SaveManager.Instance?.Get<ScreenshotModule>()?.ScreenshotFilePath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        Texture2D texture = new(2, 2);
        if (texture.LoadImage(File.ReadAllBytes(path)))
        {
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
