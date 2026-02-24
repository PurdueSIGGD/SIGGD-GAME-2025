using System;
using UnityEngine;

namespace MobCensus
{
    /// <summary>
    /// Raw data structure for storing spawn region information.
    /// Is the actual data for spawn regions that gets serialized to the save file.
    /// </summary>
    [Serializable]
    public class MobRegionDataRaw
    {
        [SerializeField] SpawnRegion.SpawnRegionState spawnRegionState;
        [SerializeField] float spawnCooldownTimer;
        public MobRegionDataRaw()
        {
            spawnCooldownTimer = 0;
            spawnRegionState = SpawnRegion.SpawnRegionState.Primed;
        }
        public MobRegionDataRaw(MobRegionDataRaw regionData)
        {
            spawnCooldownTimer = regionData.GetSpawnCooldownTimer();
            spawnRegionState = regionData.GetSpawnRegionState();
        }
        public float GetSpawnCooldownTimer()
        {
            return spawnCooldownTimer;
        }
        public void SetSpawnCooldownTimer(float spawnCooldownTimer)
        {
            this.spawnCooldownTimer = spawnCooldownTimer;
        }
        public SpawnRegion.SpawnRegionState GetSpawnRegionState()
        {
            return spawnRegionState;
        }
        public void SetSpawnRegionState(SpawnRegion.SpawnRegionState spawnRegionState)
        {
            this.spawnRegionState = spawnRegionState;
        }
    }
}