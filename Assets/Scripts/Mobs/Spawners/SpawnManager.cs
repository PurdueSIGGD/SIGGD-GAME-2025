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
    }
}
