using UnityEngine;

public class AmbientMovement : MonoBehaviour
{

    public float moveSpeed = 1.0f; // Speed of the ambient movement 
    public float rotationRate = 5f;
    public Vector3 moveDirection = Vector3.right; // Direction of the ambient movement

    float timeOffset;

    void Start()
    {
        
    }

    void Update()
    { 
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime); 
        

        timeOffset += Time.deltaTime;

        if (timeOffset > rotationRate) // Change direction every 5 seconds
        {
            transform.Rotate(0, 5, 0); // Rotate the object to face the opposite direction
            timeOffset = 0.0f; // Reset the time offset
        }
    }
}
