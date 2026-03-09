using SIGGD.Goap;
using Sirenix.OdinInspector;
using UnityEngine;

public class ApexSpawnExternalEvent: ExternalEventTriggerer
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
            //apexScript.InitializeApex(PlayerID.Instance.transform.position);
            return;
        }

        Vector3 spawnPos = Pathfinding.ERR_VECTOR;
        // find random point next to player
        for (int i = 0; i < spawnAttempts; i++)
        {
            Vector3 deviation = new();
            deviation.x = Random.Range(spawnRange.x, spawnRange.y) * (Random.value < 0.5f ? -1 : 1);
            deviation.z = Random.Range(spawnRange.x, spawnRange.y) * (Random.value < 0.5f ? -1 : 1);

            spawnPos = PlayerID.Instance.transform.position + deviation;
            spawnPos = Pathfinding.ShiftTargetToNavMesh(spawnPos, 10f);
            if (spawnPos != Pathfinding.ERR_VECTOR)
            {
                break;
            }
        }

        if (spawnPos == Pathfinding.ERR_VECTOR)
        {
            Debug.LogError("Cannot find valid spawn point for Apex after " + spawnAttempts + " attempts");
            return;
        }

        Debug.Log("Spawning apex at location: " + spawnPos);
        spawnedApex = Instantiate(apexPrefab, spawnPos, transform.rotation);
        apexScript = spawnedApex.GetComponent<Apex>();
        //apexScript.InitializeApex(PlayerID.Instance.transform.position);

        ApexSpawnConditionStrategy apexSpawnStrategy = questEventBroadcaster.conditionStrategy as ApexSpawnConditionStrategy;
        if (apexSpawnStrategy != null)
        {
            apexSpawnStrategy.SetSpawnedApex(spawnedApex);
        } else
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