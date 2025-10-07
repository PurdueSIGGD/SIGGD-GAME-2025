using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Screenshot : MonoBehaviour
{
    Image image;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<Image>();
        string screenshotPath = Application.persistentDataPath + "/screenshot.png";

        if (File.Exists(screenshotPath))
        {
            Debug.Log("LOADING!!!");
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(File.ReadAllBytes(screenshotPath));
            Debug.Log(texture.width);
            Debug.Log(texture.height);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Debug.Log(image.sprite.rect);
            Debug.Log("Loaded.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
