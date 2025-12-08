using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobCensus
{
    [Serializable]
    public class MobSpeciesRegistry
    {
        [Serializable]
        public class MobSpeciesEntry
        {
            [SerializeField] string mobId;
            [SerializeField] GameObject mobPrefab;

            public MobSpeciesEntry(string mobId, GameObject mobPrefab)
            {
                this.mobId = mobId;
                this.mobPrefab = mobPrefab;
            }

            public string GetMobId() { return mobId; }
            public GameObject GetMobPrefab() { return mobPrefab; }
        }

        [SerializeField] List<MobSpeciesEntry> speciesEntries = new List<MobSpeciesEntry>();

        public GameObject GetMobPrefabById(string mobId)
        {
            foreach (var entry in speciesEntries)
            {
                if (entry.GetMobId() == mobId)
                {
                    return entry.GetMobPrefab();
                }
            }
            return null; // Species not found
        }

        public string GetMobIdByPrefab(GameObject prefab)
        {
            foreach (var entry in speciesEntries)
            {
                if (entry.GetMobPrefab() == prefab)
                {
                    return entry.GetMobId();
                }
            }
            return null; // Species not found
        }
    }
}