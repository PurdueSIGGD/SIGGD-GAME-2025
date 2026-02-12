using System;
using System.Collections.Generic;
using CrashKonijn.Agent.Runtime;
using SIGGD.Mobs;
using UnityEngine;

namespace MobCensus
{
    [Serializable]
    public class MobSpeciesRegistry : MonoBehaviour
    {
        [Serializable]
        public class MobSpeciesEntry
        {
            string mobId = null;
            [SerializeField] GameObject mobPrefab;

            public MobSpeciesEntry(GameObject mobPrefab)
            {
                this.mobPrefab = mobPrefab;
            }

            public string GetMobId()
            {
                if (mobId == null)
                {
                    if (mobPrefab.GetComponent<HyenaBrain>() != null)
                    {
                        mobId = MobIds.hyena;
                    }
                    else if (mobPrefab.GetComponent<PreyBrain>() != null)
                    {
                        mobId = MobIds.prey;
                    }
                }
                return mobId;
            }
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