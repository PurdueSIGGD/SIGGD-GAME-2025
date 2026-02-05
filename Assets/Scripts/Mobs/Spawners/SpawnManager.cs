using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CrashKonijn.Goap.Runtime;
using SIGGD.Mobs;

using MobCensus;
public class SpawnManager : MonoBehaviour
{
    MobCensusManager mobCensus;
    MobSpeciesRegistry mobSpeciesRegistry;

    public GameObject boundaryObject;
    private Boundary boundary;
    void Awake()
    {
        boundary = boundaryObject.GetComponent<Boundary>();
    }


    public GameObject SpawnMobNew(GameObject mobPrefab, Vector3 spawnPosition)
    {
        GameObject mobObject = Instantiate(mobPrefab, spawnPosition, Quaternion.identity);
        string mobId = mobSpeciesRegistry.GetMobIdByPrefab(mobPrefab);

        // register mob in census
        mobCensus.RegisterCitizen(mobPrefab, mobObject, mobId);

        InitializeMobInternalSystems(mobObject);
        return mobObject;
    }
    // =======
    //                 spawners[i].spawnInterval += timer;
    //                 SpawnMob(spawners[i]);
    //             }
    //         }
    //     }

    //     private void SpawnMob(MobSpawner spawner)
    //     {
    //         Vector3 spawnPos = (spawner.spawnRadius != 0
    //             ? GetRandomPositionCircle(spawner.spawnPosition, spawner.spawnRadius)
    //             : spawner.spawnPosition);
    //         for (int i = 0; i < spawner.spawnCount; i++)
    //         {
    //             var agent = Instantiate(spawner.prefab, spawnPos, Quaternion.identity).GetComponent<GoapActionProvider>();
    //             agent.gameObject.SetActive(true);
    //             var agentData = agent.GetComponent<AgentData>();
    //             NavMeshQueryFilter navFilter = agentData.filter;
    //             agentData.boundary = boundary;
    //             NavMeshAgent navAgent = agent.GetComponent<NavMeshAgent>();
    //             if (!NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 15f, navFilter))
    //             {
    //                 Debug.Log($"Could not spawn {spawner.name}");
    //                 continue;
    //             }
    //             navAgent.Warp(hit.position);
    //         }
    // >>>>>>> dev
    //     }

    public GameObject SpawnMobFromSave(MobCitizenDataRaw rawData)
    {
        // pull prefab from registry
        GameObject mobPrefab = mobSpeciesRegistry.GetMobPrefabById(rawData.GetMobId());
        GameObject mobObject = Instantiate(mobPrefab, rawData.GetPosition(), Quaternion.identity);

        // populate new mob with saved serialized data
        MobCitizenPassport passport = mobObject.GetComponent<MobCitizenPassport>();
        passport.ReadMobCitizenData(rawData);

        // register mob in census
        mobCensus.RegisterCitizen(mobPrefab, mobObject, rawData.GetMobId());

        InitializeMobInternalSystems(mobObject);
        return mobObject;
    }

    void InitializeMobInternalSystems(GameObject mobObject)
    {
        // initialize goap system
        GoapActionProvider goapActionProvider = mobObject.GetComponent<GoapActionProvider>();
        goapActionProvider.gameObject.SetActive(true);

        // set boundary for territory capabillity
        AgentData agentData = mobObject.GetComponent<AgentData>();
        agentData.boundary = boundary;

        // initialize navmesh agent and validate that spawn position is within valid navmesh area
        NavMeshAgent navAgent = mobObject.GetComponent<NavMeshAgent>();
        NavMesh.SamplePosition(mobObject.transform.position, out NavMeshHit hit, 15f, NavMesh.AllAreas);
        navAgent.Warp(hit.position);
        // =======
        //         Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
        //         Vector3 candidatePos = spawnPosition + new Vector3(randomPos.x, 0f, randomPos.y);

        //         if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
        //             return hit.position;

        //         // fallback to spawnPosition if sampling fails
        //         return spawnPosition;
        // >>>>>>> dev
    }

}
