using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CrashKonijn.Goap.Runtime;
using SIGGD.Mobs;
public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private List<MobSpawner> spawners;
    private float timer;
    public GameObject boundaryObject;
    private Boundary boundary;
    void Awake()
    {
        boundary = boundaryObject.GetComponent<Boundary>();
    }
    void Update()
    {
        timer += Time.deltaTime;
        for (int i = spawners.Count - 1; i >= 0; --i) {
            if (!spawners[i].repeatSpawn)
            {
                SpawnMob(spawners[i]);
                spawners.Remove(spawners[i]);
            } else if (timer > spawners[i].spawnInterval) {

                spawners[i].spawnInterval += timer;
                SpawnMob(spawners[i]);
            }
        }
    }

    private void SpawnMob(MobSpawner spawner)
    {
        Vector3 spawnPos = (spawner.spawnRadius != 0
            ? GetRandomPositionCircle(spawner.spawnPosition, spawner.spawnRadius)
            : spawner.spawnPosition);
        for (int i = 0; i < spawner.spawnCount; i++)
        {
            var agent = Instantiate(spawner.prefab, spawnPos, Quaternion.identity).GetComponent<GoapActionProvider>();
            agent.gameObject.SetActive(true);
            var agentData = agent.GetComponent<AgentData>();
            NavMeshQueryFilter navFilter = agentData.filter;
            agentData.boundary = boundary;
            NavMeshAgent navAgent = agent.GetComponent<NavMeshAgent>();
            if (!NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 15f, navFilter))
            {
                Debug.Log($"Could not spawn {spawner.name}");
                continue;
            }
            navAgent.Warp(hit.position);
        }
    }
    private Vector3 GetRandomPositionCircle(Vector3 spawnPosition, float spawnRadius)
    {
        Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
        Vector3 candidatePos = spawnPosition + new Vector3(randomPos.x, 0f, randomPos.y);

        if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
            return hit.position;

        // fallback to spawnPosition if sampling fails
        return spawnPosition;
    }

}
