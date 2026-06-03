using UnityEngine;

public class AmbientMovement : MonoBehaviour
{

    public float moveSpeed = 1.0f; // Speed of the ambient movement 

    void Start()
    {
        // This is a placeholder for the Start method, which is called before the first frame update.
    }

    void Update()
    {
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime); 
    }
}
