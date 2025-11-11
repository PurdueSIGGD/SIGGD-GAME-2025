using System;
using UnityEngine;

public class WorkBenchScript : MonoBehaviour
{
    [SerializeField] GameObject Player;
    void OnTriggerEnter(Collider other)
    {
        if (other == Player)
        {
            Debug.Log("enter box");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
