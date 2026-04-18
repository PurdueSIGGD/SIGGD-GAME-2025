using System;
using System.Collections;
using SIGGD.Mobs;
using SIGGD.Mobs.StateMachine;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Brain for the Apex predator, built on <see cref="MobBrainBase"/>.
/// Holds all shared data and helper methods used by the Apex state machine.
/// States communicate only through this class and the <see cref="MobBrainBase.stateMachine"/>.
/// </summary>
public class Apex : MobBrainBase
{
    #region Apex References

    [Header("Apex References")]
    [Tooltip("The head bone. Assign this here and also in ApexLineOfSight — the LOS component drives it.")]
    [SerializeField] private Transform headBone;
    [Tooltip("Standalone LOS component that mirrors the head bone each frame.")]
    [SerializeField] private ApexLineOfSight lineOfSight;

    #endregion

    #region Sound References
    private readonly static string apexDamagePlayerSound = "ApexOnDamagePlayer";
    private readonly string apexOnNoticePlayerSound = "ApexOnNotice";

    #endregion

    #region Movement Settings

    [Header("Apex Movement")]
    [Tooltip("Speed multiplier used while approaching the initial alert position.")]
    [SerializeField] private float approachSpeedMulti = 1.0f;
    [Tooltip("Speed multiplier used while roaming between searches.")]
    [SerializeField] private float roamSpeedMulti = 0.6f;
    [Tooltip("Speed multiplier used while chasing a target.")]
    [SerializeField] private float chaseSpeedMulti = 1.5f;
    [Tooltip("Distance from a target position at which the Apex is considered to have arrived.")]
    [SerializeField] private float arrivalDistance = 1.5f;

    #endregion

    #region Roam Settings

    [Header("Apex Roam")]
    [Tooltip("Radius around the current guard position in which roam targets are picked.")]
    [SerializeField] private float roamRadius = 12f;
    [Tooltip("How long the Apex stays at a roam point before picking a new one.")]
    [SerializeField] private float roamPauseDuration = 2f;
    [Tooltip("How long the Apex roams before switching back to searching.")]
    [SerializeField] private float roamDuration = 8f;

    #endregion

    #region Search Settings

    [Header("Apex Search")]
    [Tooltip("Total angular sweep of the head (degrees) during a search pause.")]
    [SerializeField] private float headSweepAngle = 90f;
    [Tooltip("Time in seconds to complete one full head sweep.")]
    [SerializeField] private float headSweepDuration = 3f;
    [Tooltip("How many sweeps the Apex performs before transitioning to roaming.")]
    [SerializeField] private int sweepsBeforeRoam = 2;
    [Tooltip("Local axis the head bone rotates around during a sweep.")]
    [SerializeField] private HeadSweepAxis headSweepAxis = HeadSweepAxis.Y;

    #endregion

    #region Attack Settings

    [Header("Apex Attack")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private LayerMask attackLayerMask;
    [SerializeField] private DamageContext attackContext;

    #endregion

    #region Debug

    [Header("Apex Debug")]
    [Tooltip("When enabled, all Apex state and behaviour changes are printed to the console.")]
    [SerializeField] private bool debugLogs = false;

    /// <summary>
    /// Prints <paramref name="message"/> prefixed with "APEX: " only when <see cref="debugLogs"/> is enabled.
    /// </summary>
    public void ApexLog(string message)
    {
        if (debugLogs) Debug.Log($"APEX: {message}");
    }

    #endregion

    #region Public Accessors

    public ApexLineOfSight LineOfSight => lineOfSight;
    public float ApproachSpeedMulti => approachSpeedMulti;
    public float RoamSpeedMulti => roamSpeedMulti;
    public float ChaseSpeedMulti => chaseSpeedMulti;
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

    #region Runtime State

    /// <summary>The world position the Apex was initially alerted toward.</summary>
    public Vector3 TargetPosition { get; private set; }

    private Action onDespawn;
    private bool initialized;

    #endregion

    #region Apex States

    private ApexApproachingState approachingState;
    private ApexSearchingState searchingState;
    private ApexRoamingState roamingState;
    private ApexChasingState chasingState;
    private ApexAttackingState attackingState;
    private ApexInvestigateState investigateState;

    public ApexApproachingState ApproachingState => approachingState;
    public ApexSearchingState SearchingState => searchingState;
    public ApexRoamingState RoamingState => roamingState;
    public ApexChasingState ChasingState => chasingState;
    public ApexAttackingState AttackingState => attackingState;
    public ApexInvestigateState InvestigateState => investigateState;

    #endregion

    #region MobBrainBase Overrides

    protected override string MobName => "Apex";

    

    protected override MobContext BuildContext()
    {
        return new MobContext
        {
            Transform = transform,
            Rigidbody = GetComponent<Rigidbody>(),
            NavAgent = GetComponent<NavMeshAgent>(),
            Movement = GetComponent<Movement>(),
            AgentData = GetComponent<AgentData>(),
            Perception = GetComponent<PerceptionManager>(),
            Smell = GetComponent<Smell>(),
            type = MobType.Apex
        };
    }

    protected override void InitializeStates()
    {
        approachingState = new ApexApproachingState(this);
        searchingState = new ApexSearchingState(this);
        roamingState = new ApexRoamingState(this);
        chasingState = new ApexChasingState(this);
        attackingState = new ApexAttackingState(this);
        investigateState = new ApexInvestigateState(this);
    }

    protected override void Start()
    {
        // Don't call base — the Apex doesn't start in WanderState.
        // Initial state is set by InitializeApex(), called by ApexSpawnSystem
        // between Awake() and Start().
        if (!initialized)
        {
            ApexLog("Warning: Start() without InitializeApex(). Defaulting to approaching current position.");
            TargetPosition = PlayerID.Instance.transform.position;
            investigateState.SetTarget(TargetPosition);

            StartCoroutine(DelayedEnterInvestigate());
        }
    }

    // Wait one frame then enter investigate state.
    private IEnumerator DelayedEnterInvestigate()
    {
        yield return new WaitForSeconds(0.5f);
        stateMachine.ChangeState(InvestigateState);
    }

    protected override void EvaluateTransitions()
    {
        // Global LOS transition — if a target is spotted while not already chasing or attacking,
        // immediately switch to chasing.
        if (lineOfSight != null && lineOfSight.VisibleTarget != null)
        {
            var current = stateMachine.CurrentState;
            if (current is not ApexChasingState && current is not ApexAttackingState && PlayerID.Instance.playerHealth.CurrentHealth > 0)
            {
                ApexLog($"EvaluateTransitions — spotted '{lineOfSight.VisibleTarget.gameObject.name}', switching to ChasingState.");
                chasingState.SetTarget(lineOfSight.VisibleTarget);
                stateMachine.ChangeState(chasingState);

                // play apex notice player sound
                //AudioManager.Instance.PlayOneShotNoAsync(apexOnNoticePlayerSound, transform.position);
                AudioManager.Instance.PlayOneShotNoAsync(apexOnNoticePlayerSound, PlayerID.Instance.gameObject.transform.position);
            }
        }
    }

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
        initialized = true;

        if (lineOfSight == null)
            Debug.LogWarning("APEX: No ApexLineOfSight assigned — LOS checks will be skipped.");

        InvestigateState.SetTarget(targetPosition);
        StartCoroutine(DelayedEnterInvestigate());
        ApexLog($"Initialized. Approaching alert position {targetPosition}.");
    }

    private void OnDestroy()
    {
        onDespawn?.Invoke();
    }

    #endregion

    #region Movement Helpers

    /// <summary>
    /// Returns true when the Rigidbody is within <see cref="arrivalDistance"/> of <paramref name="target"/>.
    /// </summary>
    public bool IsAtPosition(Vector3 target)
    {
        return Vector3.Distance(ctx.Rigidbody.position, target) <= arrivalDistance;
    }

    /// <summary>
    /// Gets the NavSteering direction toward <paramref name="target"/>.
    /// </summary>
    public Vector3 GetSteeringTo(Vector3 target)
    {
        return NavSteering.GetSteeringDirection(ctx.NavAgent, ctx.Rigidbody.position, target, 0.1f);
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

    public bool IsMoving() {
        return ctx.Rigidbody.linearVelocity.magnitude > 0.1f;
    }
    #endregion

    #region Attack Helpers

    /// <summary>
    /// Performs the overlap-sphere attack — deals damage equal to target's max health
    /// to every entity in <see cref="attackRange"/>.
    /// </summary>
    public void DoAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, attackLayerMask);
        foreach (Collider col in hits)
        {
            if (col.gameObject == gameObject) continue;
            EntityHealthManager health = col.GetComponent<EntityHealthManager>();
            if (health == null) continue;

            DamageContext dmgCtx = attackContext;
            dmgCtx.attacker = gameObject;
            dmgCtx.victim = col.gameObject;
            dmgCtx.amount = health.MaxHealth;
            if (dmgCtx.victim == PlayerID.Instance.gameObject && dmgCtx.amount > 0 && PlayerID.Instance.playerHealth.CurrentHealth > 0)
            {
                AudioManager.Instance.PlayOneShotNoAsync(apexDamagePlayerSound, PlayerID.Instance.gameObject.transform.position);
            }
            health.TakeDamage(dmgCtx);
            ApexLog($"Attacked {col.gameObject.name} for {dmgCtx.amount} damage.");
        }
    }

    #endregion

    #region Gizmos

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (StateMachine != null && StateMachine.CurrentState is ApexRoamingState)
        {
            RoamingState.OnDrawGizmos();
        }
        if (StateMachine != null && StateMachine.CurrentState is ApexInvestigateState)
        {
            InvestigateState.OnDrawGizmos();
        }
    }

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

