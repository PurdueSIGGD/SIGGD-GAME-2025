using System;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public Vector3 Position = new(0, 0, 0);
    public Vector2 Rotation = new(0, 0);
    public float curHealth = 0f;
    public float curHunger = 0f;
}
