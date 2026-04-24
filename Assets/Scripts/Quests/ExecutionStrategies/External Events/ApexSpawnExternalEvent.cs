using SIGGD.Goap;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ApexSpawnExternalEvent : ExternalEventTriggerer
{
    [Tooltip("Possible spawn locations for Apex")]
    [SerializeField] Transform[] possibleSpawnPoints;

    [Tooltip("Spawn points closer than this to the target location will be ignored. Indicated by magenta wire spheres")]
    [SerializeField] float minimumSpawnDistance = 30f;

    [SerializeField] GameObject apexPrefab;
    [SerializeField] QuestEventBroadcaster questEventBroadcaster;

    Apex apexScript;
    GameObject spawnedApex = null;

    private static readonly string apexSpawnSound = "ApexOnSpawnPassiveGrow";

    public override void TriggerExternalEvent()
    {
        if (spawnedApex != null)
        {
            return;
        }

        // TODO: replace with trigger event location
        Vector3 targetPos = PlayerID.Instance.transform.position;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out UnityEngine.AI.NavMeshHit targetHit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            targetPos = targetHit.position;
        }

        Vector3 spawnPos = Pathfinding.ERR_VECTOR;
        if (possibleSpawnPoints == null || possibleSpawnPoints.Length == 0)
        {
            Debug.LogError("ApexSpawnExternalEvent: no spawn points provided — populate list of spawn points in editor");
            return;
        }

        var orderedSpawns = possibleSpawnPoints
            .Where(sp => sp != null && Vector3.Distance(sp.position, targetPos) > minimumSpawnDistance)
            .OrderBy(sp => Vector3.Distance(sp.position, targetPos));

        if (!orderedSpawns.Any())
        {
            Debug.LogError("ApexSpawnExternalEvent: no viable spawn points founds - spawn points too close to target location, adjust minimumSpawnDistance");
            return;
        }

        foreach (var sp in orderedSpawns)
        {
            Vector3 candidate = sp.position;

            // Ensure position exists on navmesh
            candidate = Pathfinding.ShiftTargetToNavMesh(candidate, 5f);
            if (candidate == Pathfinding.ERR_VECTOR) continue;

            spawnPos = candidate;
            break;
        }

        if (spawnPos == Pathfinding.ERR_VECTOR)
        {
            Debug.LogError("ApexSpawnExternalEvent: no viable spawn point found - cannot shift to navmesh");
            return;
        }

        Debug.Log("ApexSpawnExternalEvent: Spawning apex at location: " + spawnPos);
        spawnedApex = Instantiate(apexPrefab, spawnPos, transform.rotation);
        apexScript = spawnedApex.GetComponent<Apex>();
        apexScript.InitializeApex(targetPos);

        Debug.Log("Spawned Apex. Playing audio.");
        AudioManager.Instance.PlayOneShotNoAsync(apexSpawnSound, spawnPos);

        if (questEventBroadcaster.conditionStrategy is ApexSpawnConditionStrategy apexSpawnStrategy)
        {
            apexSpawnStrategy.SetSpawnedApex(spawnedApex);
        }
        else
        {
            Debug.Log("ApexSpawnExternalEvent: Apex Spawner not using Apex Spawn strategy");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (possibleSpawnPoints == null || possibleSpawnPoints.Length == 0) return;

        foreach (var sp in possibleSpawnPoints)
        {
            if (sp == null) continue;

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(sp.position, minimumSpawnDistance);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(sp.position, 1f);

            Handles.Label(sp.position + Vector3.up * 5f, "Apex Spawn Point");
        }
    }

    [Button]
    private void TestSpawn()
    {
        TriggerExternalEvent();
    }
#endif
}