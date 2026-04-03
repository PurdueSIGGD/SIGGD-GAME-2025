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


        // timer for updating raw data for citizens and spawn regions. 
        // Only needs to be updated periodically (not every frame) since it's only used for saving
        float timer = 0;
        const float UPDATE_INTERVAL = 1f; // how often to update the raw data for citizens and spawn regions (in seconds)
        private void Update()
        {
            // update raw data for citizens and spawn regions periodically for saving purposes
            
            timer += Time.deltaTime;
            if (timer >= UPDATE_INTERVAL)
            {
                timer = 0;
                foreach (MobCitizenData citizen in citizens)
                {
                    citizen.UpdateRawData();
                }
                foreach (MobRegionData region in regions)
                {
                    region.UpdateRawData();
                }
            }
        }

        /// <summary>
        /// Initializes the list of spawn regions by finding all SpawnRegion components in the scene and creating corresponding MobRegionData objects for each one.
        /// Should be called at the start of the game to populate the list of spawn regions, and can also be called when loading a save to re-initialize the spawn regions with their saved data.
        /// </summary>
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

        /// <summary>
        /// Registers a new citizen in the census. Should be called when a mob is spawned in the game world, with the prefab and instance of the mob, as well as its unique mob ID for save/load purposes.
        /// </summary>
        /// <param name="prefab">The prefab of the mob being registered.</param>
        /// <param name="instance">The instance of the mob being registered.</param>
        /// <param name="mobId">The unique ID of the mob for save/load purposes.</param>
        public void RegisterCitizen(GameObject prefab, GameObject instance, string mobId, Boundary boundary)
        {
            MobCitizenData newCitizen = new MobCitizenData(prefab, instance, mobId, boundary);
            MobCitizenPassport pass = instance.GetComponent<MobCitizenPassport>();
            pass.SetCitizenDataReference(newCitizen);
            citizens.Add(newCitizen);
        }

        /// <summary>
        /// Removes the specified citizen from the census. Should be called when a mob dies or is otherwise removed from the game world.
        /// </summary>
        /// <param name="targetCitizen"></param>
        public void RemoveCitizen(MobCitizenData targetCitizen)
        {
            citizens.Remove(targetCitizen);
        }

        /// <summary>
        /// Initializes the spawn regions for a new game. Should be called when starting a new game 
        /// to set all spawn regions to their default state (inactive with no cooldown).
        /// </summary>
        public void InitializeSpawnRegionsForNewGame()
        {
            InitializeRegionList();
            foreach (MobRegionData regionData in regions)
            {
                regionData.GetInstance().Initialize(regionData);
            }
        }
    }
}
