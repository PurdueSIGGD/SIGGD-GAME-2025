using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// To use this script, attach it to an empty gameobject with a really large box collider.
/// Anything that can collide with this box's layer will be killed if it is has a entity health manager.
/// </summary>

[RequireComponent(typeof(BoxCollider))]
public class DeathboxScript : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private LayerMask mobLayer;


    // creating the deathbox damage context.
    private DamageContext deathboxDamageContext;
    private void Start()
    {
        deathboxDamageContext = new DamageContext();
        deathboxDamageContext.attacker = gameObject;
        deathboxDamageContext.victim = gameObject;
        deathboxDamageContext.amount = 9999;
        deathboxDamageContext.xxtraContext = "Deathbox";

        gameObject.GetComponent<BoxCollider>().isTrigger = true;
    }

    // collision detection for deathbox
    private void OnTriggerEnter(Collider other)
    {
        // see if theres a way to kill this entity.
        EntityHealthManager healthManager = other.gameObject.GetComponent<EntityHealthManager>();
        bool kill_entity = true;

        // If a navmesh-agent based mob has fallen off the map, teleport them to the nearest navmesh point.

        if ((mobLayer & (1 << other.gameObject.layer)) != 0) {
            NavMeshAgent mobAgent = other.gameObject.GetComponent<NavMeshAgent>();

            if (mobAgent != null) {
                /// this is a mob, attempt to move them back onto the nav mesh instead of ending them. If successful, don't kill the mob.
                NavMeshHit hit;
                if (NavMesh.SamplePosition(other.gameObject.transform.position, out hit, Mathf.Infinity, NavMesh.AllAreas)) {
                    Vector3 closestPoint = hit.position;

                    mobAgent.Warp(closestPoint);

                    kill_entity = false;
                }
            }
        }


        // unless otherwise, if the fallen object has a healthManager, kill it.
        if (healthManager != null) {
            if (kill_entity == true) {
                deathboxDamageContext.victim = other.gameObject;
                healthManager.Die(deathboxDamageContext);
            }
        }
        // you could add something here to destroy non-entity objects, but I didn't because it could screw over someone else.
    }
}
