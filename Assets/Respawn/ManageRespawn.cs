using System;
using System.Numerics;
using CrashKonijn.Goap.Runtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ManageRespawn : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] Inventory inv;
    private UISlot[] inventory; // array (or 2D-array) for entire inventory; first 9 indices are the hotbar

    public UnityEngine.Vector3 respawnPoint;
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
            Death();
        }
    }
    public void updateSpawnPoint(Transform spawnPoint)
    {
        respawnPoint = spawnPoint.position;
    }
    
    public void Death()
    {   if (curGrave)
        {
            Destroy(curGrave);
        }
        if (!inv.IsInventoryEmpty())
        { 
            
            curGrave = Instantiate(graveObj, transform.position, transform.rotation);
            curGrave.GetComponent<graveInteract>().FillGrave(inv);
        }
        Player.transform.position = respawnPoint;
    }

}
