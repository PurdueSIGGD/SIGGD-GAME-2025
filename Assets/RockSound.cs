using Unity.VisualScripting;
using UnityEngine;

public class RockSound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Rock Sound");
        Destroy(gameObject);
    }
}
