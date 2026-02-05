using UnityEngine;
using UnityEngine.AI;
using SIGGD.Mobs;

public class SpawnPoint : MonoBehaviour
{
    // this class is just a marker for spawn regions to verify valid spawn points on first load of scene
    [SerializeField] GameObject optionalMobOverride = null;
    public bool HasMobOverride()
    {
        return optionalMobOverride == null;
    }
    public GameObject GetMobOverride()
    {
        return optionalMobOverride;
    }
}
