using UnityEngine;

public class WorkBenchScript : MonoBehaviour
{
    private GameObject player;
    private ManageRespawn respawnLogic;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            respawnLogic.UpdateSpawnPoint(transform);
            Debug.Log("RespawnPoint set at " + transform.position);
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
