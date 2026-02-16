using System.Collections.Generic;
using UnityEngine;

namespace MobCensus
{
    public class MobCensusManager : MonoBehaviour
    {
        SpawnManager spawnManager;
        [SerializeField] List<MobCitizenData> citizens = new List<MobCitizenData>();
        public List<MobCitizenData> GetCitizens() { return citizens; }

        public void Awake()
        {
            spawnManager = FindFirstObjectByType<SpawnManager>();
        }

        public void RegisterCitizen(GameObject prefab, GameObject instance, string mobId, Boundary boundary)
        {
            MobCitizenData newCitizen = new MobCitizenData(prefab, instance, mobId, boundary);
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
            foreach (MobCitizenDataRaw rawData in rawDataList)
            {
                spawnManager.SpawnMobFromSave(rawData);
            }
        }
    }
}
