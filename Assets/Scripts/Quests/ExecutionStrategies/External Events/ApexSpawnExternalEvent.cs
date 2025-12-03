using SIGGD.Goap;
using Sirenix.OdinInspector;
using UnityEngine;

public class ApexSpawnExternalEvent: ExternalEventTriggerer
{
    [Tooltip("The range from player within which the script will attempt to spawn in the apex")]
    [SerializeField, MinMaxSlider(30, 60, true)] Vector2 spawnRange;
    [Tooltip("The number of times the script will sample random points to find a valid spawn point")]
    [SerializeField] float spawnAttempts;

    [SerializeField] GameObject apexPrefab;
    public override void TriggerExternalEvent()
    {
        Vector3 spawnPos = Pathfinding.ERR_VECTOR;
        // find random point next to player
        for (int i = 0; i < spawnAttempts; i++)
        {
            spawnPos = PlayerID.Instance.transform.position + Random.insideUnitSphere * Random.Range(spawnRange.x, spawnRange.y);
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

        Debug.Log("Spawning apex");
        Instantiate(apexPrefab, spawnPos, transform.rotation);
    }
}