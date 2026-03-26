using Unity.VisualScripting;
using UnityEngine;

public class RockSound : MonoBehaviour
{
    [Tooltip("The layer mask that determines which objects are considered mobs")]
    public LayerMask mask;
    [Tooltip("Sets the radius that the rock will draw in mobs")]
    public float radius = 10;

    // Triggers when the rock hits the ground. Creates trigger box that checks for mobs within the boxRadius
    void OnCollisionEnter(Collision collision)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, mask);
        if (hits.Length > 0)
        {
            foreach (Collider hit in hits)
            {
                Debug.Log("Mob Detected");
                //This should call the mob code that tells them to move to this objects location
            }
        }
        Destroy(gameObject);
    }
}
