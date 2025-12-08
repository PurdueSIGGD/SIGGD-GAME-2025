using System;
using UnityEngine;


[CreateAssetMenu(fileName = "MobSpawner", menuName = "Scriptable Objects/MobSpawner")]
[Serializable]
public class MobSpawner : ScriptableObject
{
    public bool repeatSpawn;
    public float spawnInterval;
    public int spawnCount;
    public GameObject prefab;
    public float spawnRadius;
    public Vector3 spawnPosition;
}
