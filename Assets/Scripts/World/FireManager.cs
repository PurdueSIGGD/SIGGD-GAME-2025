using UnityEngine;
using UnityEngine.VFX;
using Utility;

/// <summary>
/// Only purpose of this fire manager is to set the correct
/// dimensions for the box collider according to the SpawnRadius
/// property in the VFX
/// </summary>
[RequireComponent (typeof(BoxCollider))]
public class FireManager : MonoBehaviour
{
    public const string SPAWN_RADIUS_PROPERTY = "Spawn Radius";
    public const string PARTICLE_SIZE_PROPERTY = "Fire Particle Size";

    public float COLLIDER_HEIGHT = 5.0f;
    
    private float SQRT2 = Mathf.Sqrt(2.0f);

    private VisualEffect fireVFX;
    private BoxCollider boxCollider;
    private float radiusAdjusted = -1.0f;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        fireVFX = GetComponentInChildren<VisualEffect>();

        SetUpCollider();
    }

    /// <summary>
    /// Set up the collider to be the correct bounds
    /// according to the "particle radius" property on VFX
    /// </summary>
    private void SetUpCollider()
    {
        radiusAdjusted = fireVFX.GetFloat(SPAWN_RADIUS_PROPERTY) + 0.5f * fireVFX.GetFloat(PARTICLE_SIZE_PROPERTY);
        float len = radiusAdjusted * SQRT2; // largest square in a circle
        boxCollider.size = new Vector3(len, COLLIDER_HEIGHT, len);
        boxCollider.center = new Vector3(
            gameObject.transform.position.x,
            gameObject.transform.position.y + 0.5f * COLLIDER_HEIGHT,
            gameObject.transform.position.z
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(gameObject.transform.position, Vector3.up, radiusAdjusted);
    }

}
