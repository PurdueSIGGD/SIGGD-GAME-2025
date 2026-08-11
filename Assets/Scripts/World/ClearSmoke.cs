using UnityEngine;
using UnityEngine.VFX;

public class ClearSmoke : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnCollisionEnter(Collision collision)
    {
        //Destroy(gameObject);
        GetComponent<VisualEffect>().Stop();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
