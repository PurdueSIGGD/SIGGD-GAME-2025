using System;
using System.Collections.Generic;
using UnityEngine;

namespace Census
{
    [Serializable]
    public class MobSpeciesRegistry
    {
        [Serializable]
        public class MobSpeciesEntry
        {
            [SerializeField] string speciesName;
            [SerializeField] GameObject speciesPrefab;

            public MobSpeciesEntry(string speciesName, GameObject speciesPrefab)
            {
                this.speciesName = speciesName;
                this.speciesPrefab = speciesPrefab;
            }

            public string GetSpeciesName() { return speciesName; }
            public GameObject GetSpeciesPrefab() { return speciesPrefab; }
        }

        [SerializeField] List<MobSpeciesEntry> speciesEntries = new List<MobSpeciesEntry>();

        public GameObject GetSpeciesPrefabByName(string speciesName)
        {
            foreach (var entry in speciesEntries)
            {
                if (entry.GetSpeciesName() == speciesName)
                {
                    return entry.GetSpeciesPrefab();
                }
            }
            return null; // Species not found
        }
    }
}