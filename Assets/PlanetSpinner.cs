using JetBrains.Annotations;
using UnityEngine;

public class PlanetSpinner : MonoBehaviour
{

    public float xRate = 1f;
    public float yRate = 1f;
    public float zRate = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        transform.Rotate(xRate * Time.fixedDeltaTime, yRate * Time.fixedDeltaTime, zRate * Time.fixedDeltaTime);
    }
}
