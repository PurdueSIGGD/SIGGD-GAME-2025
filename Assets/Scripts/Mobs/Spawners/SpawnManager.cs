using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using MobCensus;
using SIGGD.Mobs;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.AI;
public class SpawnManager : MonoBehaviour
{
    MobCensusManager mobCensus;
    MobSpeciesRegistry mobSpeciesRegistry;

    public GameObject boundaryObject;
    [SerializeField]
    private float spawnPointRadius = 3f;
    void Awake()
    {
        mobCensus = FindFirstObjectByType<MobCensusManager>();
        mobSpeciesRegistry = FindFirstObjectByType<MobSpeciesRegistry>();
    }


    public GameObject SpawnMobNew(GameObject mobPrefab, Vector3 spawnPosition, Boundary boundary)
    {
        GameObject mobObject = Instantiate(mobPrefab, spawnPosition, Quaternion.identity);
        string mobId = mobSpeciesRegistry.GetMobIdByPrefab(mobPrefab);

        // register mob in census
        mobCensus.RegisterCitizen(mobPrefab, mobObject, mobId, boundary);

        InitializeMobInternalSystems(mobObject, boundary, mobId);
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
        mobCensus.RegisterCitizen(mobPrefab, mobObject, rawData.GetMobId(), rawData.GetBoundary());

        InitializeMobInternalSystems(mobObject, rawData.GetBoundary(), rawData.GetMobId());
        return mobObject;
    }

    void InitializeMobInternalSystems(GameObject mobObject, Boundary boundary = null, string mobId = null)
    {
        // initialize goap system
        GoapActionProvider goapActionProvider = mobObject.GetComponent<GoapActionProvider>();
        goapActionProvider.gameObject.SetActive(true);

        // set boundary for territory capabillity (optional depending on if mob has this capability)
        AgentData agentData = mobObject.GetComponent<AgentData>();
        if (agentData != null)
        {
            if (boundary != null)
            {
                agentData.boundary = boundary;
            }
            agentData.SetMobId(mobId);
        }
        // initialize navmesh agent and validate that spawn position is within valid navmesh area
        NavMeshAgent navAgent = mobObject.GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError($"{mobObject.name} missing NavMeshAgent");
            return;
        }

        navAgent.updatePosition = false;
        navAgent.updateRotation = false;
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = navAgent.agentTypeID,
            areaMask = NavMesh.AllAreas
        };
        NavMeshQueryFilter navFilter = agentData.filter;
        bool success = NavMesh.SamplePosition(mobObject.transform.position, out NavMeshHit hit, 15f, navFilter);
        navAgent.Warp(hit.position);
        if (success) {
            Debug.Log("success");
        } else
        {
            Debug.Log("failure");
        }
    }
}
