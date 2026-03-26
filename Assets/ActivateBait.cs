using UnityEngine;
using UnityEngine.UI;

public class ActivateBait : MonoBehaviour
{
    public float radius = 10f;
    public float duration = 10f;

    [Tooltip("The layer mask that determines which objects are considered mobs")]
    public LayerMask mask;

    // After bait is thrown initializes the radius and duration.
    public void Initialize(float radius, float duration)
    {
        this.radius = radius;
        this.duration = duration;
        //Debug.Log($"Bait initialized with radius: {radius} and duration: {duration}");
    }

    // Triggers when the bait hits the ground. Creates trigger sphere that checks for mobs within the radius
    void OnCollisionEnter(Collision collision)
    {
        Destroy(GetComponent<Rigidbody>()); // Remove the Rigidbody to stop the bait from moving after it hits the ground
        Destroy(GetComponent<BoxCollider>());
        
        
        //Collider[] hits = Physics.OverlapSphere(transform.position, radius, mask);
        //if (hits.Length > 0)
        //{
        //    foreach (Collider hit in hits)
        //    {
        //        Debug.Log("Mob Detected");
        //        /* This should call the mob code that tells them to move to this objects location
        //           Durration should be passed to the mob to know how long they should be attracted to the bait
        //           After the duration is completed the bait should be destroyed */
        //    }
        //}
    }
    void OnTriggerEnter(Collider other)
    {

        if ((mask.value & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log("Mob detected");
            // This should call the mob code that tells them to move to this objects location
        }
    }
}
