using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{

    public float sensX;
    public float sensY;

    public Transform orientation;
    float xRotation;
    float yRotation;

    public float minY = -30;
    public float maxY = 30;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
 
       
        float mouseX =  PlayerInput.Instance.lookInput.x * Time.deltaTime * sensX;
        float mouseY =  PlayerInput.Instance.lookInput.y * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);
        
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        





        
    }
}
