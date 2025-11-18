using System;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class WorkBenchScript : MonoBehaviour
{
    GameObject Player;
    //[SerializedField] GameObject Player;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.GetComponent<ManageRespawn>().updateSpawnPoint(transform);
            Debug.Log("RespawnPoint set");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
