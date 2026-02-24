using System;
using UnityEngine;

namespace MobCensus
{
    [Serializable]
    public class MobRegionData
    {
        [SerializeField] SpawnRegion instance;
        [SerializeField] MobRegionDataRaw rawData;

        public MobRegionData(SpawnRegion instance)
        {
            this.instance = instance;
            rawData = new MobRegionDataRaw();
        }
        public SpawnRegion GetInstance()
        {
            return instance;
        }
        public void UpdateRawData()
        {
            float spawnCooldown = instance.GetSpawnCooldownTimer();
            rawData.SetSpawnCooldownTimer(spawnCooldown);
        }
        public MobRegionDataRaw GetRawData()
        {
            return rawData;
        }
    }
}