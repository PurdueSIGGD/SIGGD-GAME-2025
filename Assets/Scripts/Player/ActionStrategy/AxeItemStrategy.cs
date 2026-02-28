using System.Collections.Generic;
using UnityEngine;

public class AxeItemStrategy : IPlayerActionStrategy
{
    [Header("Axe Attributes")]
    [SerializeField] private float axeDamage = 5f;
    [Tooltip("how long the action animation plays before the hitbox")]
    [SerializeField] private float axeAnimation_HitboxDelay = 0f;
    [Tooltip("how long the hitbox stays after hitboxDelay before disappearing.")]
    [SerializeField] private float axeAnimation_HitboxDuration = 0.5f;

    [Header("Axe Raycast (Size)")]
    [Tooltip("This is the 'range' of the axe attack")]
    [SerializeField] private float raycastMagnitude = 1f;
    [Tooltip("The axe attacks in a cone raycast. This is the radius of this cone where 90 = half circle.")]
    [Range(0f, 90f)]
    [SerializeField] private float raycastConeAngle = 75;
    [Tooltip("how many rings subset the cone raycast.")]
    [Range(3, 50)]
    [SerializeField] private int raycastRingSubsections = 10;
    [Tooltip("How many rays there are per ring. Should be pretty high.")]
    [Range(3, 50)]
    [SerializeField] private int raycastRingRays = 10;

    [Header("Axe Raycast (Targets)")]
    [SerializeField] private LayerMask validRaycastLayers;


    private float hitbox_timer = -99f;

    // during an axe swing, these are all the hit colliders. This clears itself when swinging again.
    private List<Collider> hitColliders = new List<Collider>();

    // this is a reference to some transform that faces the player's camera direction
    private Transform handsTransform;

    protected override void OnEnter()
    {
        // this won't actually break the script, its just so that the attributes make sense.

        if (axeAnimation_HitboxDelay + axeAnimation_HitboxDuration > ActionDuration) {
            Debug.LogError("ERR: AxeItem animation DELAY and DURATION are longer than its action duration.");
        }

        // block any attempts to axe swing while the animation is playing

        if (hitbox_timer - axeAnimation_HitboxDuration - axeAnimation_HitboxDelay + ActionDuration > 0f) {
            return;
        }

        // do the stuff

        base.OnEnter();
        handsTransform = PlayerHands.instance.transform;
        PlayHandAction(); // plays animation for axe

        hitbox_timer = axeAnimation_HitboxDelay + axeAnimation_HitboxDuration;
        hitColliders.Clear();
    }

    protected override void OnUpdate() {
        base.OnUpdate();

        // If hitbox_timer is above hitboxDuration, then it is in the 'delay' part of the animation
        if (hitbox_timer > 0f) {
            if (hitbox_timer < axeAnimation_HitboxDuration) {
                TriggerHitbox();
            }
        }

        hitbox_timer -= Time.deltaTime;
    }

    /// <summary>
    /// returns true if the hitbox hit something and false if not
    /// </summary>
    /// <returns></returns>
    private bool TriggerHitbox()
    {
        Vector3 forwardVector = handsTransform.forward;
        Vector3 rightVector = handsTransform.right;
        Vector3 origin = handsTransform.position;

        int rings = raycastRingSubsections;
        int raysPerRing = raycastRingRays;

        for (int r = 1; r <= rings; r++)
        {
            float ringAngle = (((float) r) / rings) * raycastConeAngle;
            float angleStep = 360f / raysPerRing;

            for (int i = 0; i < raysPerRing; i++)
            {
                Quaternion ringRotation = Quaternion.AngleAxis(i * angleStep, forwardVector);

                // tilt ray out
                Quaternion tiltRotation = Quaternion.AngleAxis(ringAngle, rightVector);

                Vector3 rayDirection = ringRotation * tiltRotation * forwardVector;

                // use this to debug the ray
                //Debug.DrawRay(origin, rayDirection * raycastMagnitude, Color.red, .1f);

                RaycastHit hit;
                if (Physics.Raycast(origin, rayDirection, out hit, raycastMagnitude, validRaycastLayers))
                {
                    if (!hitColliders.Contains(hit.collider)) {
                        hitColliders.Add(hit.collider);
                        GameObject hitObject = hit.transform.gameObject;
                        EntityHealthManager healthManager = hitObject.GetComponent<EntityHealthManager>();

                        if (healthManager != null) {
                            // found a valid health manager, now kill it pow wabam.
                            DamageContext damageContext = new DamageContext();
                            damageContext.attacker = PlayerID.Instance.gameObject;
                            damageContext.victim = hitObject;
                            damageContext.amount = axeDamage;
                            healthManager.TakeDamage(damageContext);
                        }
                    }
                }
                
            }
        }

        return hitColliders.Count > 0;
    }
}
