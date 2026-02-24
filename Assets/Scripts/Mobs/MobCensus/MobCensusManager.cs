using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace MobCensus
{
    public class MobCensusManager : MonoBehaviour
    {
        SpawnManager spawnManager;
        [SerializeField] List<MobRegionData> regions = new List<MobRegionData>();
        [SerializeField] List<MobCitizenData> citizens = new List<MobCitizenData>();
        public List<MobRegionData> GetRegions() { return regions; }
        public List<MobCitizenData> GetCitizens() { return citizens; }

        public void Awake()
        {
            spawnManager = FindFirstObjectByType<SpawnManager>();
            InitializeRegionList();
        }
        public void InitializeRegionList()
        {
            regions.Clear();
            SpawnRegion[] foundRegions = FindObjectsByType<SpawnRegion>(FindObjectsSortMode.InstanceID);
            foreach (SpawnRegion region in foundRegions)
            {
                MobRegionData regionData = new MobRegionData(region);
                regions.Add(regionData);
            }
        }
        public void RegisterCitizen(GameObject prefab, GameObject instance, string mobId)
        {
            MobCitizenData newCitizen = new MobCitizenData(prefab, instance, mobId);
            MobCitizenPassport pass = instance.GetComponent<MobCitizenPassport>();
            pass.SetCitizenDataReference(newCitizen);
            citizens.Add(newCitizen);
        }
        public void RemoveCitizen(MobCitizenData targetCitizen)
        {
            citizens.Remove(targetCitizen);
        }
        public void LoadRawDataFromSave(List<MobCitizenDataRaw> rawDataList)
        {
            Debug.Log("SPAWNING MOBS FROM SAVE: " + rawDataList.Count);
            foreach (MobCitizenDataRaw rawData in rawDataList)
            {
                spawnManager.SpawnMobFromSave(rawData);
            }
        }
        public void LoadSpawnRegionsFromSave(List<MobRegionDataRaw> rawDataList)
        {
            Debug.Log("LOADING SPAWN REIGONS: " + rawDataList.Count);

            InitializeRegionList();
            Debug.Assert(rawDataList != null && rawDataList.Count == regions.Count);
            for (int i = 0; i < regions.Count; i++)
            {
                MobRegionData myRegionData = regions[i];
                float cooldown = rawDataList[i].GetSpawnCooldownTimer();
                myRegionData.GetRawData().SetSpawnCooldownTimer(cooldown);
                myRegionData.GetInstance().Initialize(cooldown);
            }
        }
    }
}
