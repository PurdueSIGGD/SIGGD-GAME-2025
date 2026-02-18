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
        [SerializeField] float spawnCooldownTimer;
        public MobRegionDataRaw()
        {
            spawnCooldownTimer = 0;
        }
        public float GetSpawnCooldownTimer()
        {
            return spawnCooldownTimer;
        }
        public void SetSpawnCooldownTimer(float spawnCooldownTimer)
        {
            this.spawnCooldownTimer = spawnCooldownTimer;
        }
    }
}