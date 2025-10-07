using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System;

public class ExitInputHandler : MonoBehaviour
{
    public new Camera camera;

    public static Action onSave;

    InputAction jumpAction = InputSystem.actions.FindAction("Jump");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jumpAction.performed += OnJump;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("HELLO BRO I AM GOING TO EXIT NOW");

        onSave.Invoke();

        int width = camera.pixelWidth, height = camera.pixelHeight;

        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        camera.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] pngData = screenshot.EncodeToPNG();
        using (FileStream stream = File.Create(Application.persistentDataPath + "/screenshot.png", pngData.Length))
        {
            stream.Write(pngData);
        }
        Destroy(screenshot);

        Debug.Log("OKAY I WILL ACTUALLY EXIT NOW");

        Application.Quit();
    }
}
