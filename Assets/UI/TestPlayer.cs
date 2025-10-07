using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
struct PlayerInfo
{
    public float xRot;
    public float yRot;
}

public class TestPlayer : MonoBehaviour
{
    InputAction lookAction;
    public new Camera camera;

    float yRot = 0;
    float xRot = 0;

    string savePath;
    public float mouseMul = 10.0f;

    PlayerInfo info
    {
        get
        {
            return new PlayerInfo {xRot = xRot, yRot = yRot};
        }
        set
        {
            xRot = value.xRot;
            yRot = value.yRot;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        savePath = Application.persistentDataPath + "/player.json";
        if (File.Exists(savePath))
        {
            Debug.Log("READING player.json");
            info = JsonUtility.FromJson<PlayerInfo>(File.ReadAllText(savePath));
        }

        Cursor.lockState = CursorLockMode.Locked;
        lookAction = InputSystem.actions.FindAction("Look");

        ExitInputHandler.onSave += Save;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        Vector2 lookVector = lookAction.ReadValue<Vector2>();

        yRot -= lookVector.y * dt * mouseMul;
        // Debug.Log(yRot);
        // yRot = Mathf.Clamp(yRot, 90, -90);
        camera.transform.localEulerAngles = new Vector3(yRot, 0.0f, 0.0f);

        xRot += lookVector.x * dt * mouseMul;
        xRot = Mathf.Repeat(xRot, 360.0f);
        transform.localEulerAngles = new Vector3(0.0f, xRot, 0.0f);
    }

    void Save()
    {
        Debug.Log("OH NO QUITTING!!!! PLAYER WILL BE SAVED.");

        File.WriteAllText(savePath, JsonUtility.ToJson(info));
    }
}
