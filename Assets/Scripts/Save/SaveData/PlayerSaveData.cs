using System;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public Vector3 Position;
    public Vector3 Rotation;
    public PlayerStats Stats;
    public EntityHealthManager HealthManager;
}
