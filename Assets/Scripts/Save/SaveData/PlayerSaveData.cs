using System;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public Vector3 Position = new(0, 0, 0);
    public Vector2 Rotation = new(0, 0);
    public PlayerStats Stats;
    public EntityHealthManager HealthManager;
}
