using System;
using Extensions.StateMachine;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Root MonoBehaviour for the Apex predator. Holds all shared data and helper
/// methods used by the state machine. States are never aware of each other —
/// they communicate only through this class and the <see cref="stateController"/>.
/// </summary>
public class Apex : MonoBehaviour
{
    #region State Machine

    public StateController<ApexState> stateController;

    #endregion

    #region References

    [Header("References")]
    [SerializeField] private NavMeshAgent navMeshAgent;
    [Tooltip("The head bone. Assign this here and also in ApexLineOfSight — the LOS component drives it.")]
    [SerializeField] private Transform headBone;
    [Tooltip("Standalone LOS component that mirrors the head bone each frame.")]
    [SerializeField] private ApexLineOfSight lineOfSight;

    #endregion

    #region Movement Settings

    [Header("Movement Settings")]
    [Tooltip("Speed used while approaching the initial alert position.")]
    [SerializeField] private float approachSpeed = 4f;
    [Tooltip("Speed used while roaming between searches.")]
    [SerializeField] private float roamSpeed = 2.5f;
    [Tooltip("Speed used while chasing a target.")]
    [SerializeField] private float chaseSpeed = 6f;

    #endregion

    #region Roam Settings

    [Header("Roam Settings")]
    [Tooltip("Radius around the current guard position in which roam targets are picked.")]
    [SerializeField] private float roamRadius = 12f;
    [Tooltip("How long the Apex stays at a roam point before picking a new one.")]
    [SerializeField] private float roamPauseDuration = 2f;
    [Tooltip("How long the Apex roams before switching back to searching.")]
    [SerializeField] private float roamDuration = 8f;

    #endregion

    #region Search Settings

    [Header("Search Settings")]
    [Tooltip("Total angular sweep of the head (degrees) during a search pause.")]
    [SerializeField] private float headSweepAngle = 90f;
    [Tooltip("Time in seconds to complete one full head sweep.")]
    [SerializeField] private float headSweepDuration = 3f;
    [Tooltip("How many sweeps the Apex performs before transitioning to roaming.")]
    [SerializeField] private int sweepsBeforeRoam = 2;
    [Tooltip("Local axis the head bone rotates around during a sweep. X = nod up/down, Y = turn left/right, Z = tilt side to side.")]
    [SerializeField] private HeadSweepAxis headSweepAxis = HeadSweepAxis.Y;

    #endregion

    #region Attack Settings

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private LayerMask attackLayerMask;
    [SerializeField] private DamageContext attackContext;

    #endregion

    #region Debug

    [Header("Debug")]
    [Tooltip("When enabled, all Apex state and behaviour changes are printed to the console.")]
    [SerializeField] private bool debugLogs = false;

    /// <summary>
    /// Prints <paramref name="message"/> prefixed with "APEX: " only when <see cref="debugLogs"/> is enabled.
    /// Call this from any state using <c>apex.ApexLog(...)</c>.
    /// </summary>
    public void ApexLog(string message)
    {
        if (debugLogs) Debug.Log($"APEX: {message}");
    }

    #endregion

    #region Runtime State

    /// <summary>The world position the Apex was initially alerted toward.</summary>
    public Vector3 TargetPosition { get; private set; }

    private Action onDespawn;

    #endregion

    #region Public Accessors

    /// <summary>Returns the LOS component, or null if not assigned in the inspector.</summary>
    public ApexLineOfSight LineOfSight => lineOfSight;
    public float RoamRadius => roamRadius;
    public float RoamPauseDuration => roamPauseDuration;
    public float RoamDuration => roamDuration;
    public float HeadSweepAngle => headSweepAngle;
    public float HeadSweepDuration => headSweepDuration;
    public int SweepsBeforeRoam => sweepsBeforeRoam;
    public HeadSweepAxis HeadSweepAxis => headSweepAxis;
    public float AttackRange => attackRange;
    public LayerMask AttackLayerMask => attackLayerMask;
    public DamageContext AttackContext => attackContext;

    #endregion

    #region Initialization

    /// <summary>
    /// Called by <see cref="ApexSpawnSystem"/> immediately after instantiation.
    /// </summary>
    /// <param name="targetPosition">World position of the alerting action.</param>
    /// <param name="despawnCallback">Invoked when this Apex is destroyed so the spawn system can reset.</param>
    public void InitializeApex(Vector3 targetPosition, Action despawnCallback = null)
    {
        TargetPosition = targetPosition;
        onDespawn = despawnCallback;

        if (lineOfSight == null)
            Debug.LogWarning("APEX: No ApexLineOfSight assigned — LOS checks will be skipped. Assign it in the inspector.");

        stateController = new StateController<ApexState>(this);
        stateController.ChangeState(new ApexApproachingState(this));
        ApexLog($"Initialized. Approaching alert position {targetPosition}.");
    }

    private void OnDestroy()
    {
        onDespawn?.Invoke();
    }

    #endregion

    #region Movement Helpers

    /// <summary>Sends the NavMeshAgent toward <paramref name="target"/> at approach speed.</summary>
    public void MoveTowardTarget(Vector3 target)
    {
        navMeshAgent.SetDestination(target);
        navMeshAgent.speed = approachSpeed;
        navMeshAgent.updateRotation = true;
    }

    /// <summary>Sends the NavMeshAgent toward <paramref name="target"/> at roam speed.</summary>
    public void RoamTowardTarget(Vector3 target)
    {
        navMeshAgent.SetDestination(target);
        navMeshAgent.speed = roamSpeed;
        navMeshAgent.updateRotation = true;
    }

    /// <summary>Sends the NavMeshAgent toward <paramref name="target"/> at chase speed.</summary>
    public void ChaseTarget(Vector3 target)
    {
        navMeshAgent.SetDestination(target);
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.updateRotation = true;
    }

    /// <summary>Cancels any active NavMesh path and stops the agent.</summary>
    public void StopMoving()
    {
        navMeshAgent.ResetPath();
    }

    /// <returns>True when the agent has arrived at its current destination.</returns>
    public bool IsAtTarget()
    {
        return !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    }

    /// <summary>
    /// Picks a random point on the NavMesh within <paramref name="radius"/> of <paramref name="origin"/>.
    /// </summary>
    public bool TryGetRoamPoint(Vector3 origin, float radius, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 candidate = origin + UnityEngine.Random.insideUnitSphere * radius;
            candidate.y = origin.y;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius * 0.5f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = origin;
        return false;
    }

    #endregion

    #region Attack Helpers

    /// <summary>
    /// Performs the overlap-sphere attack — deals damage equal to target's max health
    /// to every entity in <see cref="attackRange"/>. Called by the attacking state.
    /// </summary>
    public void DoAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, attackLayerMask);
        foreach (Collider col in hits)
        {
            if (col.gameObject == gameObject) continue;
            EntityHealthManager health = col.GetComponent<EntityHealthManager>();
            if (health == null) continue;

            DamageContext ctx = attackContext;
            ctx.attacker = gameObject;
            ctx.victim = col.gameObject;
            ctx.amount = health.MaxHealth;
            health.TakeDamage(ctx);
            ApexLog($"Attacked {col.gameObject.name} for {ctx.amount} damage.");
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #endregion
}

/// <summary>Which local axis the head bone rotates around during a search sweep.</summary>
public enum HeadSweepAxis
{
    X,
    Y,
    Z
}

