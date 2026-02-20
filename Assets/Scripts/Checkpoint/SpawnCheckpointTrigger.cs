using UnityEngine;

public class SpawnCheckpointTrigger : MonoBehaviour
{
    GameObject checkpoint;

    void Awake()
    {
        Instantiate(checkpoint, transform.position, transform.rotation);    
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 8);   
    }
}
