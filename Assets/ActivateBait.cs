using System;
using System.Collections;
using SIGGD.Mobs.StateMachine;
using UnityEngine;

public class ActivateBait : MonoBehaviour
{
    // Radius and Duration both set on each bait types item info prefab
    public bool actOnApex = true;
    public float radius = 10f;
    public float duration = 10f;
    public float baitDuration = 3f;

    [Tooltip("The layer mask that determines which objects are considered mobs")]
    public LayerMask mask;

    // After bait is thrown initializes the radius and duration.
    public void Initialize(float radius, float duration, float baitDuration)
    {
        this.radius = radius;
        this.duration = duration;
        this.baitDuration = baitDuration;
    }

    // Triggers when the bait hits the ground. Creates trigger sphere that checks for mobs within the radius
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            return; // Ignore collisions with the player
        }
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 90f); // Rotate the bait to lay flat on the ground
        Destroy(GetComponent<Rigidbody>()); // Remove the Rigidbody to stop the bait from moving after it hits the ground
        Destroy(GetComponent<BoxCollider>());
        StartCoroutine(DestroyAfterDuration());
    }

    public void OnBaitTriggerEnter(Collider other)
    {
        Debug.Log("[MobBrainBase] OnBaitTriggerEnter");
        if (!actOnApex && other.CompareTag("Apex")) return;
        
        Debug.Log("[MobBrainBase] Entering baited state with bait: " + other.name);

        if (other.CompareTag("Predator") || (actOnApex && other.CompareTag("Apex")))
        {
            Debug.Log("[ActivateBait] Detected mob within radius: " + other.gameObject.name);
            MobBrainBase mobBrain = other.GetComponentInParent<MobBrainBase>();
            if (mobBrain == null)
            {
                return;
            }

            mobBrain.EnterBaitedState(gameObject, baitDuration);
        }
    }

    private IEnumerator DestroyAfterDuration()
    {
        yield return new WaitForSeconds(duration); // Wait for the specified duration
        // If needed , you can add code here to notify mobs that the bait is no longer active before destroying it
        Destroy(gameObject);
    }
}