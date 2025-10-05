using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    InputAction lookAction;
    public new Camera camera;
    float yRot = 0;
    float xRot = 0;

    public float mouseMul = 10.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        lookAction = InputSystem.actions.FindAction("Look");
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        Vector2 lookVector = lookAction.ReadValue<Vector2>();

        yRot -= lookVector.y * dt * mouseMul;
        Debug.Log(yRot);
        // yRot = Mathf.Clamp(yRot, 90, -90);
        camera.transform.localEulerAngles = new Vector3(yRot, 0.0f, 0.0f);

        xRot += lookVector.x * dt * mouseMul;
        xRot = Mathf.Repeat(xRot, 360.0f);
        transform.localEulerAngles = new Vector3(0.0f, xRot, 0.0f);
    }
}
