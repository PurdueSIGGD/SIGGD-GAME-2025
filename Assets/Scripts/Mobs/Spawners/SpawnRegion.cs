using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnRegion : MonoBehaviour
{
    public class SpawnRateData
    {
        public GameObject mobPrefab;
        public float spawnWeight;
    }
    SpawnManager spawnManager;
    [SerializeField] List<SpawnRateData> spawnList;
    [SerializeField] List<SpawnPoint> spawnPoints;

    [SerializeField] float spawnCooldown;
    [SerializeField] float proportionSpawnedMin;
    [SerializeField] float proportionSpawnedMax;

    float spawnCooldownTimer;

    bool playerInRegion = false; // toggled true when player enters region, toggled false when player exits region
    bool spawnTriggered = false; // toggled true when player enters region, toggled false when cooldown ends

    void Start()
    {
        spawnManager = FindFirstObjectByType<SpawnManager>();
        ScanChildrenForSpawnPoints();
        spawnCooldownTimer = spawnCooldown;
    }

    void Update()
    {
        if (spawnTriggered == true && playerInRegion == false)
        {
            spawnCooldownTimer -= Time.deltaTime;
            if (spawnCooldownTimer <= 0f)
            {
                spawnTriggered = false;
                spawnCooldownTimer = spawnCooldown;
            }
        }
    }

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRegion = true;
        }
        else
        {
            return; // only care about player entering region
        }

        if (!spawnTriggered)
        {
            spawnTriggered = true;
            SpawnMobsInRegion();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && spawnTriggered == true)
        {
            playerInRegion = false;
        }
    }

    void SpawnMobsInRegion()
    {
        List<SpawnPoint> spawnPointsCopy = new(spawnPoints);
        int numSpawn = UnityEngine.Random.Range(
            Mathf.FloorToInt(spawnPoints.Count * proportionSpawnedMin),
            Mathf.CeilToInt(spawnPoints.Count * proportionSpawnedMax)
        );
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

            spawnManager.SpawnMobNew(mobPrefab, spawnPoint.transform.position);
        }
    }

    GameObject GetRandomMobPrefab()
    {
        float totalChance = 0f;
        foreach (SpawnRateData spawnData in spawnList)
        {
            totalChance += spawnData.spawnWeight;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalChance);
        float cumulativeChance = 0f;

        foreach (SpawnRateData spawnData in spawnList)
        {
            cumulativeChance += spawnData.spawnWeight;
            if (randomValue <= cumulativeChance)
            {
                return spawnData.mobPrefab;
            }
        }

        return spawnList[0].mobPrefab; // default to 1st mob just in case, should never reach here
    }

}