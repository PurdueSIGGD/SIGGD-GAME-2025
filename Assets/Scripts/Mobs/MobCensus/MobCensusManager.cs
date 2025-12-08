using System.Collections.Generic;
using UnityEngine;

namespace MobCensus
{
    public class MobCensusManager : MonoBehaviour
    {
        SpawnManager spawnManager;
        [SerializeField] List<MobCitizenData> citizens = new List<MobCitizenData>();
        public List<MobCitizenData> GetCitizens() { return citizens; }

        public void Start()
        {
            spawnManager = FindFirstObjectByType<SpawnManager>();
        }

        public void RegisterCitizen(GameObject prefab, GameObject instance, string mobId)
        {
            MobCitizenData newCitizen = new MobCitizenData(prefab, instance, mobId);
            citizens.Add(newCitizen);
        }
        public void RemoveCitizen(MobCitizenData targetCitizen)
        {
            citizens.Remove(targetCitizen);
        }

        public void LoadRawDataFromSave(List<MobCitizenDataRaw> rawDataList)
        {
            foreach (MobCitizenDataRaw rawData in rawDataList)
            {
                spawnManager.SpawnMobFromSave(rawData);
            }
        }
    }
}
