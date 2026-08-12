using System;
using UnityEngine;

public class AmbientMovement : MonoBehaviour
{

    public float moveSpeed = 1.0f; // Speed of the ambient movement 
    public float rotationRate = 5f;
    
    float timeOffset;

    void Start()
    {
        
    }

    void Update()
    { 
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime); 
        

        timeOffset += Time.deltaTime;

        if (timeOffset > rotationRate) 
        {
            Int32 rotateStrength = UnityEngine.Random.Range(1, 3);
            transform.Rotate(0, rotateStrength, 0); 
            timeOffset = 0.0f; // Reset the time offset
        }
    }
}
