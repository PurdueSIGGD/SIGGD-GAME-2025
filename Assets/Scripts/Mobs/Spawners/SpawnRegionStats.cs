using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "SpawnRegionStats", menuName = "ScriptableObjects/SpawnRegionStats", order = 1)]
public class SpawnRegionStats : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public List<SpawnRegion.SpawnRateData> SpawnRates { get; private set; }
    [field: SerializeField] public float SpawnCooldown { get; private set; }
    [field: SerializeField] public float MinPropSpawned { get; private set; }
    [field: SerializeField] public float MaxPropSpawned { get; private set; }
    [field: SerializeField] public float Radius { get; private set; }
}