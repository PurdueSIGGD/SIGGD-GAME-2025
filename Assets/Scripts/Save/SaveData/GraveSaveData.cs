using UnityEngine;

public class GraveSaveData
{
    // Scene the grave belongs to. GraveModule.Apply only respawns the grave when this matches
    // the currently active scene — a grave dropped in ShipScene must never appear in NathanA0.
    public string sceneName = string.Empty;

    public Vector3 position;
    public Quaternion rotation;
    public string[] names;
    public int[] count;

}
