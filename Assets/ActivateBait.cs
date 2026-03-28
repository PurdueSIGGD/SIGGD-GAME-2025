using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActivateBait : MonoBehaviour
{
    // Radius and Duration both set on each bait types item info prefab
    public float radius = 10f;
    public float duration = 10f;

    [Tooltip("The layer mask that determines which objects are considered mobs")]
    public LayerMask mask;

    // After bait is thrown initializes the radius and duration.
    public void Initialize(float radius, float duration)
    {
        this.radius = radius;
        this.duration = duration;
    }

    // Triggers when the bait hits the ground. Creates trigger sphere that checks for mobs within the radius
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            return; // Ignore collisions with the player
        }
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0); // Rotate the bait to lay flat on the ground
        Destroy(GetComponent<Rigidbody>()); // Remove the Rigidbody to stop the bait from moving after it hits the ground
        Destroy(GetComponent<BoxCollider>());
        StartCoroutine(DestroyAfterDuration());
    }

    void OnTriggerEnter(Collider other)
    {

        if ((mask.value & (1 << other.gameObject.layer)) != 0)
        {
            // This should call the mob code that tells them to move to this objects location
        }
    }

    private IEnumerator DestroyAfterDuration()
    {
        yield return new WaitForSeconds(duration); // Wait for the specified duration
        // If needed , you can add code here to notify mobs that the bait is no longer active before destroying it
        Destroy(gameObject);
    }
}
