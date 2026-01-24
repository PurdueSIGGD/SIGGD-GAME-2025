using UnityEngine;
using UnityEngine.AI;
using SIGGD.Mobs;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] GameObject mobPrefab;
    bool spawned = false;
    private GameObject thingSpawned = null;
    [SerializeField] float spawnChance = 0.5f;
    public Boundary boundary;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !spawned && Random.value <= spawnChance)
        {
            thingSpawned = Instantiate(mobPrefab, transform.position, Quaternion.identity);
            var agentData = thingSpawned.GetComponent<AgentData>();
            NavMeshQueryFilter navFilter = agentData.filter;
            agentData.boundary = boundary;
            NavMeshAgent navAgent = thingSpawned.GetComponent<NavMeshAgent>();
            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 15f, navFilter))
            {
                return;
            }
            navAgent.Warp(hit.position);
            Debug.Log("Mob Spawned at Spawn Point");
            thingSpawned.SetActive(true);
            spawned = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && spawned)
        {
            spawned = false;
            Destroy(thingSpawned);
            Debug.Log("Player Exited Spawn Point Area");
        }
    }
}
