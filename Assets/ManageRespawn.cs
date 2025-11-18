using System;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ManageRespawn : MonoBehaviour
{
    [SerializeField] GameObject Player;
    UnityEngine.Vector3 respawnPoint;
    Boolean respawnSet = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            if (!respawnSet)
            {
                Debug.Log("no respawnpoint set");
            }
            else
            {
                Player.transform.position = respawnPoint;
                Debug.Log("Respawned");

            }
        }
    }
    public void updateSpawnPoint(Transform spawnPoint)
    {
        respawnPoint = spawnPoint.position;
        respawnSet = true;
    }

}
