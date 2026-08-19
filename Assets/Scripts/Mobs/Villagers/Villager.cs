using System;
using FMOD;
using FMOD.Studio;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Villager : MonoBehaviour {
    [SerializeField] Animator animator;
    [SerializeField] private Boundary boundary;
    [SerializeField] private float unboundTravelRadius;
    [SerializeField] private float minIdleTime = 2f;
    [SerializeField] private float maxIdleTime = 5f;
    [SerializeField] private float maxTravelTime = 10f;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] GameObject corpse;
    
    EventInstance footsteps = new EventInstance();
    
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
    }
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.stoppingDistance = stoppingDistance;

    }

    private void Start()
    {
        PickNewDestination();
        FMODEvents.Instance.GetEventInstance("SlugWalk", instance => { footsteps = instance; });
    }

    void OnEnable()
    {
        EntityHealthManager.OnDeath += OnSlugDeath;
    }

    private void OnDisable()
    {
        EntityHealthManager.OnDeath -= OnSlugDeath;
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

    private void FixedUpdate()
    {
        //UpdateFootstepSound();
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
    
    private void UpdateFootstepSound()
    {
        if (!isIdle)
        {
            // NOTE: 3d attributes need to be set in order to play instances in 3d
            //ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(rb.position, rb.linearVelocity, rb.linearVelocity / rb.linearVelocity.magnitude, rb.transform.up);
            ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(transform.position, Vector3.zero, transform.forward, Vector3.up);
            footsteps.set3DAttributes(attr);

            PLAYBACK_STATE playbackState;
            footsteps.getPlaybackState(out playbackState);

            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                footsteps.start();
            }
        }
        else
        {
            footsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
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
