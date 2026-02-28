using UnityEngine;

public class SpawnWorkBenchTrigger : MonoBehaviour
{
    GameObject respawnBench;

    void Awake()
    {
        Instantiate(respawnBench, transform.position, transform.rotation);    
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 8);   
    }
}
