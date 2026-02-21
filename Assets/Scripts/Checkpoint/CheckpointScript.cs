using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    private GameObject player;
    private ManageRespawn respawnLogic;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            respawnLogic.UpdateSpawnPoint(transform);
            SaveManager.Instance.Save(); // Save
            Debug.Log("RespawnPoint set at " + transform.position);
            Debug.Log("Reached checkpoint - Saved");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = PlayerID.Instance.gameObject;
        respawnLogic = player.GetComponent<ManageRespawn>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
