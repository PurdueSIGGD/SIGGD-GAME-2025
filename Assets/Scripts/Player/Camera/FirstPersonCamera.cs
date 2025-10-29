using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Camera sensitivity")]
    [SerializeField] float sensX;
    [SerializeField] float sensY;

    [Header("Camera view bounds")]
    [SerializeField] float minY = -30;
    [SerializeField] float maxY = 30;

    private bool canControl = true;

    // Public Transform orientation;
    float xRotation;
    float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (canControl)
        {
            float mouseX = PlayerInput.Instance.lookInput.x * Time.deltaTime * sensX;
            float mouseY = PlayerInput.Instance.lookInput.y * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minY, maxY);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            //orientation.rotation = Quaternion.Euler(0, yRotation, 0);   
        }
    }

    private void EnableCamRotation() => canControl = true;
    private void DisableCamRotation() => canControl = false;
}
