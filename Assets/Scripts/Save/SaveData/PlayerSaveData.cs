using System;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public Vector3 Position = new(480.5f, 9.33f, -4.1f); // temp solution of not setting player to 0, 0, 0 if there's no save data
    public Vector2 Rotation = new(0, 0);
    //public PlayerStats Stats;
    public EntityHealthManager HealthManager;
}
