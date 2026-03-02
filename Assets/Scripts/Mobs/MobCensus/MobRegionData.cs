using System;
using UnityEngine;

namespace MobCensus
{
    [Serializable]
    public class MobRegionData
    {
        [SerializeField] SpawnRegion instance;
        [SerializeField] MobRegionDataRaw rawData;

        public MobRegionData(SpawnRegion instance, MobRegionDataRaw rawData = null)
        {
            this.instance = instance;
            this.rawData = rawData ?? new MobRegionDataRaw();
        }
        public SpawnRegion GetInstance()
        {
            return instance;
        }
        public void UpdateRawData()
        {
            rawData.SetSpawnRegionState(instance.GetCurrentState());
            rawData.SetSpawnCooldownTimer(instance.GetSpawnCooldownTimer());
        }
        public MobRegionDataRaw GetRawData()
        {
            return rawData;
        }
    }
}