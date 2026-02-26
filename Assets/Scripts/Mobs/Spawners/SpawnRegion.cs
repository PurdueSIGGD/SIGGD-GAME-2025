using System;
using System.Collections.Generic;
using MobCensus;
using UnityEngine;

[Serializable]
public class SpawnRegion : MonoBehaviour
{
    [Serializable]
    public class SpawnRateData
    {
        public GameObject mobPrefab;
        public float spawnWeight;
    }
    public enum SpawnRegionState
    {
        Primed, // player has never entered region or hasn't entered in a while, spawn is not ready and will trigger on player entry
        Active, // player is currently in region, spawn is ready and will trigger immediately, or spawn just triggered and is now on cooldown until next trigger
        Cooldown // player has left region but spawn is still on cooldown, spawn is not ready and will trigger on player entry once cooldown ends
    }
    [SerializeField] SpawnRegionStats regionStats;

    [SerializeField] SpawnRegionState currentState;
    SpawnManager spawnManager;
    [SerializeField] List<SpawnPoint> spawnPoints;

    [Header("SpawnRegionSphereSettings")]
    float spawnRegionCheckIntervalSec = 1; // how often to check if player is in region when player is not currently in region (either inactive or on cooldown)
    float spawnRegionCheckTimer = 0;
    [SerializeField] Transform centerPosition;
    float spawnCooldownTimer;

    bool initialized = false;
    public float GetSpawnCooldownTimer()
    {
        return spawnCooldownTimer;
    }
    const float NULL_CONST = -676869;

    /// <summary>
    /// Initializes the spawn region. If a MobRegionData object is provided, the spawn region will be initialized with its data.
    /// </summary>
    /// <param name="mobRegionData">The MobRegionData object containing the data for initialization.</param>
    public void Initialize(MobRegionData mobRegionData = null)
    {
        ScanChildrenForSpawnPoints();
        spawnManager = FindFirstObjectByType<SpawnManager>();
        if (mobRegionData != null)
        {
            currentState = mobRegionData.GetRawData().GetSpawnRegionState();
            spawnCooldownTimer = mobRegionData.GetRawData().GetSpawnCooldownTimer();
        }
        else
        {
            currentState = SpawnRegionState.Primed;
        }
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        if (currentState == SpawnRegionState.Active)
        {
            if (!IsPlayerInRegion())
            {
                SetState(SpawnRegionState.Cooldown);
            }
        }

        if (currentState == SpawnRegionState.Primed)
        {
            spawnRegionCheckTimer -= Time.deltaTime;
            if (spawnRegionCheckTimer <= 0)
            {
                print("SPAWNREGION -> CHECKING FOR PLAYER IN REGION " + regionStats.Name);
                spawnRegionCheckTimer = spawnRegionCheckIntervalSec;
                if (IsPlayerInRegion())
                {
                    SetState(SpawnRegionState.Active);
                }
            }
        }

        if (currentState == SpawnRegionState.Cooldown)
        {
            spawnCooldownTimer -= Time.deltaTime;
            if (spawnCooldownTimer <= 0)
            {
                if (IsPlayerInRegion())
                {
                    SetState(SpawnRegionState.Active);
                }
                else
                {
                    SetState(SpawnRegionState.Primed);
                }
            }
        }
    }

    /// <summary>
    /// Sets the state of the spawn region and handles necessary logic for each state change (like starting cooldowns or spawning mobs)
    /// </summary>
    /// <param name="newState"></param>
    void SetState(SpawnRegionState newState)
    {
        if (newState == SpawnRegionState.Primed)
        {
            // not on cooldown (no special actions)
        }
        else if (newState == SpawnRegionState.Active)
        {
            SpawnMobsInRegion();
            spawnCooldownTimer = regionStats.SpawnCooldown; // start cooldown
        }
        else if (newState == SpawnRegionState.Cooldown)
        {
            spawnCooldownTimer = regionStats.SpawnCooldown; // start cooldown
        }
        currentState = newState;
    }
    public SpawnRegionState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// Scans children of the spawn region for spawn points and adds them to the spawn point pool.
    /// </summary>
    void ScanChildrenForSpawnPoints()
    {
        spawnPoints.Clear();
        foreach (Transform child in transform)
        {
            SpawnPoint pt = child.GetComponent<SpawnPoint>();
            if (pt != null)
            {
                spawnPoints.Add(pt);
            }
        }
    }

    /// <summary>
    /// Checks if player is in the spawn region by doing a sphere check for the player layer, 
    /// and then verifying that the collider found belongs to the player (just in case)
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerInRegion()
    {
        LayerMask myLayers = LayerMask.GetMask("Player");
        Collider[] results = Physics.OverlapSphere(centerPosition.position, regionStats.Radius, myLayers);
        foreach (Collider c in results)
        {
            if (c.gameObject == PlayerID.Instance.gameObject)
            {
                print("SPAWNREGION -> PLAYER IN REGION " + regionStats.Name);
                return true;
            }
        }
        return false;
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPosition.position, regionStats.Radius);
    }
    /// <summary>
    /// Selects a random proportion of spawnpoints in the pool to spawn a random mob at (unless there's an override)
    /// </summary>
    void SpawnMobsInRegion()
    {
        List<SpawnPoint> spawnPointsCopy = new(spawnPoints);
        int numSpawn = UnityEngine.Random.Range(
            Mathf.FloorToInt(spawnPoints.Count * regionStats.MinPropSpawned),
            Mathf.CeilToInt(spawnPoints.Count * regionStats.MaxPropSpawned)
        );
        print("MYSPAWN: " + numSpawn);
        for (int i = 0; i < numSpawn; i++)
        {
            if (spawnPointsCopy.Count == 0) break; // no more spawn points available

            int spawnPointIndex = UnityEngine.Random.Range(0, spawnPointsCopy.Count);
            SpawnPoint spawnPoint = spawnPointsCopy[spawnPointIndex];
            spawnPointsCopy.RemoveAt(spawnPointIndex);

            GameObject mobPrefab;
            if (spawnPoint.HasMobOverride())
                mobPrefab = spawnPoint.GetMobOverride();
            else
                mobPrefab = GetRandomMobPrefab();
            print("MYSPAWN AHHHHH");
            spawnManager.SpawnMobNew(mobPrefab, spawnPoint.transform.position);
        }
    }
    GameObject GetRandomMobPrefab()
    {
        float totalChance = 0f;
        foreach (SpawnRateData spawnData in regionStats.SpawnRates)
        {
            totalChance += spawnData.spawnWeight;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalChance);
        float cumulativeChance = 0f;

        foreach (SpawnRateData spawnData in regionStats.SpawnRates)
        {
            cumulativeChance += spawnData.spawnWeight;
            if (randomValue <= cumulativeChance)
            {
                return spawnData.mobPrefab;
            }
        }

        return regionStats.SpawnRates[0].mobPrefab; // default to 1st mob just in case, should never reach here
    }
}