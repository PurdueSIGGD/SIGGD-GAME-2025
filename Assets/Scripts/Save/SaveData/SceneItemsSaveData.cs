using System;
using System.Collections.Generic;
using OneOf.Types;
using UnityEngine;

[Serializable]
public class SceneItemsSaveData
{
    // Scene the position/rotation/respawn fields belong to. PlayerModule.Apply pushes those
    // fields onto the live player only when this matches the currently active scene — so a
    // save taken in ShipScene never teleports the player when they load into NathanA0.
    // Stats (health/hunger/stamina/radiation/gloves/slime) are considered scene-agnostic and
    // are applied regardless.
    public string sceneName = string.Empty;

    public bool villageDestroyed = false;

    // Item names of the music boxes already picked up, so loading a scene does not respawn them
    public List<string> collectedMusicBoxes = new();
}
