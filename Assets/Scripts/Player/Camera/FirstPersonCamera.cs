using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Camera sensitivity")]
    [SerializeField] float sensX;
    [SerializeField] float sensY;

    [Header("Camera view bounds")]
    [SerializeField] float minY = -30;
    [SerializeField] float maxY = 30;

    float xRotation;
    float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX =  PlayerInput.Instance.lookInput.x * Time.deltaTime * sensX;
        float mouseY =  PlayerInput.Instance.lookInput.y * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    public void SetRotation(Vector2 rot)
    {
        xRotation = rot.x;
        yRotation = rot.y;
    }

    public Vector2 GetRotation()
    {
        return new Vector2(xRotation, yRotation);
    }
}
