using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class Villager : MonoBehaviour {
    [SerializeField] Animator animator;
    [SerializeField] private Boundary boundary;
    [SerializeField] private float unboundTravelRadius;
    [SerializeField] private float minIdleTime = 2f;
    [SerializeField] private float maxIdleTime = 5f;
    [SerializeField] private float maxTravelTime = 10f;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] GameObject corpse;
    [Button] void kill()
    {
        GetComponent<EntityHealthManager>().TakeDamage(new DamageContext { amount = 9999, attacker = null, victim = gameObject });
    }

    private NavMeshAgent navAgent;
    private float travelTimer;
    private float idleTimer;
    private float idleDuration;
    private bool isIdle;
    
    
    private void OnSlugDeath(DamageContext damageContext)
    {
        if (damageContext.victim != gameObject) return;// ignore self-inflicted damage
        // Spawn corpse at current position and rotation
        Instantiate(corpse, transform.position, transform.rotation);
        // Destroy the villager game object
        Destroy(gameObject);
    }
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.stoppingDistance = stoppingDistance;

    }

    private void Start()
    {
        PickNewDestination();
        EntityHealthManager.OnDeath += OnSlugDeath;
    }

    private void Update()
    {

        if (isIdle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
                PickNewDestination();
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
            travelTimer += Time.deltaTime;

            // Timeout � destination is probably unreachable
            if (travelTimer >= maxTravelTime)
            {
                PickNewDestination();
                return;
            }

            // Arrived
            if (!navAgent.pathPending && navAgent.remainingDistance <= stoppingDistance)
            {
                navAgent.ResetPath();
                isIdle = true;
                idleTimer = 0f;
                idleDuration = Random.Range(minIdleTime, maxIdleTime);
            }
        }
    }

    private void PickNewDestination()
    {
        Vector3 destination = boundary != null
            ? LocateRandomPositionWithinBoundary()
            : LocateRandomPosition();

        navAgent.SetDestination(destination);
        travelTimer = 0f;
        isIdle = false;
    }

    // Copied from WanderTargetSensor
    private Vector3 LocateRandomPositionWithinBoundary()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * boundary.MaxDist;
            Vector2 query = boundary.Centroid + randomOffset;
            if (boundary.IsInBoundary(query))
                return new Vector3(query.x, transform.position.y, query.y);
        }
        return new Vector3(boundary.Centroid.x, transform.position.y, boundary.Centroid.y);
    }

    private Vector3 LocateRandomPosition()
    {
        var randomInCircle = Random.insideUnitCircle * unboundTravelRadius;
        return transform.position + new Vector3(randomInCircle.x, 0f, randomInCircle.y);
    }
}
