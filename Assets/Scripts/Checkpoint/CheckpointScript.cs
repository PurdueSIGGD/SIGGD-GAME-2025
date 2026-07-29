using SIGGD.Save;
using UnityEditor;
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
            // Checkpoint save: gated by GameStateManager (req. 1) — no need to force through a chase.
            SaveManager.Instance?.SaveGameplay(SaveTrigger.Checkpoint);
            Debug.Log("Reached checkpoint, respawnPoint set at " + transform.position);
        }
    }

    void Start()
    {
        player = PlayerID.Instance.gameObject;
        respawnLogic = player.GetComponent<ManageRespawn>();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 1);
#if UNITY_EDITOR
        Handles.Label(transform.position + Vector3.up * 1.5f, "Checkpoint");
#endif
    }
}
