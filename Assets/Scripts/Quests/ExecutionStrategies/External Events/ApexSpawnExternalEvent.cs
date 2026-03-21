using SIGGD.Goap;
using Sirenix.OdinInspector;
using UnityEngine;

public class ApexSpawnExternalEvent : ExternalEventTriggerer
{
    [Tooltip("Hardset point to spawn the apex at, will override random spawning")]
    [SerializeField] Vector3 spawnPosition;
    [Tooltip("The range from player within which the script will attempt to spawn in the apex")]
    [SerializeField, MinMaxSlider(30, 60, true)] Vector2 spawnRange;
    [Tooltip("The number of times the script will sample random points to find a valid spawn point")]
    [SerializeField] float spawnAttempts;

    [SerializeField] GameObject apexPrefab;
    [SerializeField] QuestEventBroadcaster questEventBroadcaster;

    Apex apexScript;
    GameObject spawnedApex = null;
    public override void TriggerExternalEvent()
    {
        if (spawnedApex != null)
        {
            return;
        }

        if (spawnPosition != default)
        {
            Debug.Log("Spawning apex at location: " + spawnPosition);
            apexScript = Instantiate(apexPrefab, spawnPosition, transform.rotation).GetComponent<Apex>();
            apexScript.InitializeApex(PlayerID.Instance.transform.position);
            return;
        }

        Vector3 targetPos = PlayerID.Instance.transform.position;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out UnityEngine.AI.NavMeshHit targetHit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            targetPos = targetHit.position;
        }

        Vector3 spawnPos = Pathfinding.ERR_VECTOR;
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

        // Local function to try finding a random point in a specific range bounds
        bool TryFindValidSpot(float rangeMin, float rangeMax, out Vector3 validPos)
        {
            Vector3 deviation = new Vector3();
            deviation.x = Random.Range(rangeMin, rangeMax) * (Random.value < 0.5f ? -1f : 1f);
            deviation.z = Random.Range(rangeMin, rangeMax) * (Random.value < 0.5f ? -1f : 1f);

            Vector3 candidatePos = targetPos + deviation;

            Vector3 rayOrigin = candidatePos + Vector3.up * 100f;
            // Ignore triggers (like large Music, Post-Processing, or Room volumes) that often cover the whole scene
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, Mathf.Infinity, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
            {
                candidatePos = groundHit.point;
            }

            candidatePos = Pathfinding.ShiftTargetToNavMesh(candidatePos, 5f);


            if (candidatePos != Pathfinding.ERR_VECTOR)
            {
                // Verify we can actually pathfind to the player from this random spot
                if (UnityEngine.AI.NavMesh.CalculatePath(candidatePos, targetPos, UnityEngine.AI.NavMesh.AllAreas, path))
                {
                    if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        validPos = candidatePos;
                        return true;
                    }
                }
            }
            validPos = Pathfinding.ERR_VECTOR;
            return false;
        }

        // 1. Initial attempts at strictly defined spawn range bounds
        for (int i = 0; i < spawnAttempts; i++)
        {
            if (TryFindValidSpot(spawnRange.x, spawnRange.y, out spawnPos))
                break;
        }

        // 2. Fallback attempts: Search again but incrementally massively expand the range outwards each time
        if (spawnPos == Pathfinding.ERR_VECTOR)
        {
            Debug.Log($"Apex initial spawn attempts failed. Falling back to expanding range...");
            for (int j = 0; j < 3; j++)
            {
                float expansion = (j + 1) * 10f;
                for (int i = 0; i < spawnAttempts; i++)
                {
                    // Push the search ring further and further inward linearly on each attempt
                    if (TryFindValidSpot(spawnRange.x - expansion, spawnRange.y - expansion, out spawnPos))
                    {
                        Debug.Log($"Apex successfully found an expanded spawn point bounded at -{expansion} units.");
                        break;
                    }
                }
                if (spawnPos != Pathfinding.ERR_VECTOR) break;
            }
        }

        if (spawnPos == Pathfinding.ERR_VECTOR)
        {
            Debug.LogError($"Cannot find valid spawn point for Apex even after standard and expanded attempts.");
            return;
        }

        Debug.Log("Spawning apex at location: " + spawnPos);
        spawnedApex = Instantiate(apexPrefab, spawnPos, transform.rotation);
        apexScript = spawnedApex.GetComponent<Apex>();
        apexScript.InitializeApex(targetPos);

        if (questEventBroadcaster.conditionStrategy is ApexSpawnConditionStrategy apexSpawnStrategy)
        {
            apexSpawnStrategy.SetSpawnedApex(spawnedApex);
        }
        else
        {
            Debug.Log("Apex Spawner not using Apex Spawn strategy");
        }
    }


    [Button]
    private void TestSpawn()
    {
        TriggerExternalEvent();
    }

}