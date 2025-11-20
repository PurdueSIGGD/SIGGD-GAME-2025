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
    public GameObject graveObj;
    GameObject curGrave = null;
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
                death();
            }
        }
    }
    public void updateSpawnPoint(Transform spawnPoint)
    {
        respawnPoint = spawnPoint.position;
        respawnSet = true;
    }
    
    public void death()
    {   
        if (curGrave)
        {
            Destroy(curGrave);
        }
        curGrave = Instantiate(graveObj, transform.position, transform.rotation);
        Player.transform.position = respawnPoint;
        Debug.Log("Respawned");
    }

}
