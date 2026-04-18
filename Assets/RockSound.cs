using Unity.VisualScripting;
using UnityEngine;

public class RockSound : MonoBehaviour
{
    public LayerMask mask;
    public Collider trigger;
    [Tooltip("Sets the size of the trigger box from its center to each side")]
    public Vector3 boxRadius = new Vector3(5,5,5);
    
    // Triggers when the rock hits the ground. Creates trigger box that checks for mobs within the boxRadius
    void OnCollisionEnter(Collision collision)
    {
        Collider[] hits = Physics.OverlapBox(transform.position, boxRadius, new Quaternion(0,0,0,0), mask);
        if (hits.Length > 0)
        {
            foreach (Collider hit in hits)
            {
                if (hit.gameObject.CompareTag("Predator"))
                {
                    Debug.Log("Mob Detected");
                }
                //This should call the mob code that tells them to move to this objects location
            }
        }
        Destroy(gameObject);
    }
}
