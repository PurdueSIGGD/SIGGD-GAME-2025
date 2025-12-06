using UnityEngine;

public class ManageRespawn : MonoBehaviour
{
    private GameObject player;
    private Inventory inv;

    public Vector3 respawnPoint;
    public GameObject graveObj;
    GameObject curGrave = null;

    void Start()
    {
        player = PlayerID.Instance.gameObject;
        inv = PlayerID.Instance.Inventory;
    }

    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Death();
        }
#endif
    }
    public void UpdateSpawnPoint(Transform spawnPoint)
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
            curGrave.GetComponent<GraveInteract>().FillGrave(inv);
        }
        player.transform.position = respawnPoint;
    }

}
