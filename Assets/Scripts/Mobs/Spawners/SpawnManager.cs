using CrashKonijn.Goap.Runtime;
using MobCensus;
using SIGGD.Mobs;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A general centralized interface for spawning mobs.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    MobCensusManager mobCensus;
    MobSpeciesRegistry mobSpeciesRegistry;

    [SerializeField]
    void Awake()
    {
        mobCensus = FindFirstObjectByType<MobCensusManager>();
        mobSpeciesRegistry = FindFirstObjectByType<MobSpeciesRegistry>();
    }

    public void Start()
    {
        mobCensus.InitializeSpawnRegionsForNewGame();
    }

    public GameObject SpawnMobNew(GameObject mobPrefab, Vector3 spawnPosition, Boundary boundary)
    {
        GameObject mobObject = Instantiate(mobPrefab, spawnPosition, Quaternion.identity);
        string mobId = mobSpeciesRegistry.GetMobIdByPrefab(mobPrefab);

        // register mob in census
        mobCensus.RegisterCitizen(mobPrefab, mobObject, mobId, boundary);
        MobCitizenPassport passport = mobObject.GetComponent<MobCitizenPassport>();
        if (passport == null)
        {
            Debug.LogError($"Mob prefab {mobPrefab.name} is missing a MobCitizenPassport component.");
            return mobObject;
        }
        passport.SetMobCensusReference(mobCensus);

        InitializeMobInternalSystems(mobObject, boundary, mobId);
        return mobObject;
    }

    void InitializeMobInternalSystems(GameObject mobObject, Boundary boundary = null, string mobId = null)
    {
        if (mobObject == null)
        {
            Debug.LogError($"Mob gameobject is null");
            return;
        }

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
        NavMeshQueryFilter navFilter = new NavMeshQueryFilter
        {
            agentTypeID = navAgent.agentTypeID,
            areaMask = NavMesh.AllAreas
        };
        if (agentData != null && agentData.filter.areaMask != 0)
            navFilter = agentData.filter;
        bool success = NavMesh.SamplePosition(mobObject.transform.position, out NavMeshHit hit, 5f, navFilter);

        if (success)
        {
            mobObject.transform.position = hit.position;
            navAgent.Warp(hit.position);
            navAgent.nextPosition = hit.position;
            navAgent.ResetPath();
            navAgent.isStopped = false;
            var rb = mobObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                rb.isKinematic = true;
                rb.useGravity = false;
                rb.position = hit.position;
            }

            Debug.Log("success");
        }
        else
        {
            Debug.Log("failure");
        }

        // initialize goap system
        GoapActionProvider goapActionProvider = mobObject.GetComponent<GoapActionProvider>();
        if (goapActionProvider == null)
        {
            Debug.LogWarning("Mob does not have a goap action provider", mobObject);
            return;
        }
        goapActionProvider.gameObject.SetActive(true);
    }
}
