using System;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    // Scene the position/rotation/respawn fields belong to. PlayerModule.Apply pushes those
    // fields onto the live player only when this matches the currently active scene — so a
    // save taken in ShipScene never teleports the player when they load into NathanA0.
    // Stats (health/hunger/stamina/radiation/gloves/slime) are considered scene-agnostic and
    // are applied regardless.
    public string sceneName = string.Empty;

    public Vector3 Position = new(480.5f, 7.2f, -4.1f); // temp solution of not setting player to 0, 0, 0 if there's no save data
    public Vector2 Rotation = new(0, 0);
    public float curHealth = -1f;
    public float curHunger = -1f;
    public float curStamina = -1f;
    public bool staminaDisabled = false;
    public bool hasGloves = false;
    public Vector3 RespawnPosition = new(480.5f, 7.2f, -4.1f);
    public int slimeLevel = 4;
    public float radiationLevel = 0f;
}
