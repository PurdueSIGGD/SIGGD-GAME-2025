using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using CrashKonijn.Goap.Runtime;
public class SpawnManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private List<MobSpawner> spawners;
    private float timer;
    void Start()
    {

    }
    void Update()
    {
        timer += Time.deltaTime;
        for (int i = spawners.Count - 1; i >= 0; --i) {
            Debug.Log(i);
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
    // Update is called once per frame
    private void SpawnMob(MobSpawner spawner)
    {
        if (spawner.spawnRadius != 0) {
            for (int i = 0; i < spawner.spawnCount; i++)
            {
                var agent = Instantiate(spawner.prefab, GetRandomPositionCircle(spawner.spawnPosition, spawner.spawnRadius), Quaternion.identity).GetComponent<GoapActionProvider>();
                agent.gameObject.SetActive(true);
                NavMeshAgent navAgent = agent.GetComponent<NavMeshAgent>();
                NavMeshHit hit;
                if (!navAgent.Raycast(Vector3.down, out hit))
                {
                    Debug.Log($"Could not spawn {spawner.name}");
                    continue;
                }
                navAgent.Warp(hit.position);
            }
        } else {
            for (int i = 0; i < spawner.spawnCount; i++)
            {
                var agent = Instantiate(spawner.prefab, spawner.spawnPosition, Quaternion.identity).GetComponent<GoapActionProvider>();
                agent.gameObject.SetActive(true);
                NavMeshAgent navAgent = agent.GetComponent<NavMeshAgent>();
                NavMeshHit hit;
                if (!navAgent.Raycast(Vector3.down, out hit))
                {
                    Debug.Log($"Could not spawn {spawner.name}");
                    continue;
                }
                navAgent.Warp(hit.position);
            }
        }
    }
    private Vector3 GetRandomPositionCircle(Vector3 spawnPosition, float spawnRadius)
    {
        Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
        return spawnPosition + (new Vector3(randomPos.x, -1, randomPos.y));
    }
}
