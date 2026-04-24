using System;
using System.Collections;
using SIGGD.Mobs;
using SIGGD.Mobs.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Sirenix.OdinInspector;

/// <summary>
/// Brain for the Apex predator, built on <see cref="MobBrainBase"/>.
/// Holds all shared data and helper methods used by the Apex state machine.
/// States communicate only through this class and the <see cref="MobBrainBase.stateMachine"/>.
/// </summary>
public class Apex : MobBrainBase
{
    private static readonly int WalkingHash = Animator.StringToHash("Walking");
    private static readonly int AttackingHash = Animator.StringToHash("Attacking");
    #region Apex References

    [Header("Apex References")]
    [Tooltip("The head bone. Assign this here and also in ApexLineOfSight — the LOS component drives it.")]
    [SerializeField] private Transform headBone;
    [Tooltip("Standalone LOS component that mirrors the head bone each frame.")]
    [SerializeField] private ApexLineOfSight lineOfSight;
    [SerializeField] private Animator animator;
    [SerializeField] private PerceptionManager perceptionManager;
    [SerializeField] private Smell smell;

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
    [SerializeField, MinMaxSlider(0f, 100f)] private Vector2 roamRadius = new Vector2(10f, 90f);
    [Tooltip("How long the Apex stays at a roam point before picking a new one.")]
    [SerializeField, MinMaxSlider(0, 20)] private Vector2 roamPauseDuration = new(2, 6);
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
    [SerializeField] private float attackRange = 15.0f;
    [SerializeField] private float maxLungeSpeed = 22f;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float minFlightTime = 0.30f;
    [SerializeField] private float windupTime = 0.15f;
    [SerializeField] private LayerMask attackLayerMask;
    [SerializeField] private DamageContext attackContext;
    [Tooltip("Time to wait and mog at target before giving up attack, if attack cannot reach")]
    [SerializeField] private float findPathBuffer = 5f;

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
    public Vector2 RoamRadius => roamRadius;
    public Vector2 RoamPauseDuration => roamPauseDuration;
    public float RoamDuration => roamDuration;
    public float HeadSweepAngle => headSweepAngle;
    public float HeadSweepDuration => headSweepDuration;
    public int SweepsBeforeRoam => sweepsBeforeRoam;
    public HeadSweepAxis HeadSweepAxis => headSweepAxis;
    public float AttackRange => attackRange;
    public float MaxLungeSpeed => maxLungeSpeed; 
    public float ArcHeight => arcHeight;
    public float MinFlightTime => minFlightTime;
    public float WindupTime => windupTime;
    public LayerMask AttackLayerMask => attackLayerMask;
    public DamageContext AttackContext => attackContext;

    #endregion

    #region Runtime State

    /// <summary>The world position the Apex was initially alerted toward.</summary>
    public Vector3 TargetPosition { get; private set; }

    private Action onDespawn;
    private bool initialized;
    private NavMeshPath cachedPath;

    #endregion

    #region Apex States

    //private ApexApproachingState approachingState;
    //private ApexSearchingState searchingState;
    private ApexRoamingState roamingState;
    private ApexChasingState chasingState;
    private ApexAttackingState attackingState;
    private ApexInvestigateState investigateState;
    private MoggingState moggingState;

    //public ApexApproachingState ApproachingState => approachingState;
    //public ApexSearchingState SearchingState => searchingState;
    public ApexRoamingState RoamingState => roamingState;
    public ApexChasingState ChasingState => chasingState;
    public ApexAttackingState AttackingState => attackingState;
    public ApexInvestigateState InvestigateState => investigateState;
    public MoggingState MoggingState => moggingState;

    #endregion

    #region MobBrainBase Overrides

    protected override string MobName => "Apex";

    private readonly string apexOnNoticePlayerSound = "ApexOnNotice";
    private static readonly string apexLosePlayerSound = "ApexOnLosePlayer";

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
            Type = MobType.Apex,
            Animator = animator
        };
    }

    protected override void InitializeStates()
    {
        //approachingState = new ApexApproachingState(this);
        //searchingState = new ApexSearchingState(this);
        roamingState = new ApexRoamingState(this);
        chasingState = new ApexChasingState(this);
        attackingState = new ApexAttackingState(this);
        investigateState = new ApexInvestigateState(this);
        moggingState = new MoggingState(this, findPathBuffer);
        baitedState = new BaitedState(ctx, stateMachine, investigateState, baitMoveSpeedMultiplier, baitTurnResponsiveness, baitArrivalDistance);
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
        if (stateMachine.CurrentState == baitedState)
        {
            if (baitedState.returnToSender)
            {
                stateMachine.ChangeState(wanderState);
            }
            return;
        }
        
        // Global LOS transition — if a target is spotted while not already chasing or attacking,
        // immediately switch to chasing.
        ApexTarget target = HasVisibleTarget();
        if (target != null)
        {
            var current = stateMachine.CurrentState;
            if (current is not ApexChasingState && current is not ApexAttackingState)
            {
                ApexLog($"EvaluateTransitions — spotted '{target.gameObject.name}', switching to ChasingState.");
                chasingState.SetTarget(target);
                stateMachine.ChangeState(chasingState);

                // play apex notice player sound
                if (target.gameObject == PlayerID.Instance.gameObject)
                {
                    AudioManager.Instance.PlayOneShotNoAsync(apexOnNoticePlayerSound, transform.position);
                }
            }
        }
    }

    private ApexTarget HasVisibleTarget()
    {
        if (perceptionManager != null)
        {
            if (perceptionManager.CanSeePlayer && perceptionManager.PlayerTarget != null)
            {
                ApexTarget target = perceptionManager.PlayerTarget.GetComponent<ApexTarget>();
                if (target != null)
                {
                    return target;
                }
                else
                {
                    Debug.LogError("Missing ApexTarget component on player target. Likely lost to merge again, please add");
                    return null;
                }
            }

            if (perceptionManager.preyTargets != null && perceptionManager.preyTargets.Count > 0)
            {
                return perceptionManager.preyTargets[0].GetComponent<ApexTarget>();
            }
        }
        if (smell != null)
        {
            if (smell.PlayerTarget != null)
            {
                ApexTarget target = smell.PlayerTarget.GetComponent<ApexTarget>();
                if (target != null)
                {
                    return target;
                }
                else
                {
                    Debug.LogError("Missing ApexTarget component on player target. Likely lost to merge again, please add");
                    return null;
                }
            }

            if (smell.ClosestPrey != null)
            {
                return smell.ClosestPrey.GetComponent<ApexTarget>();
            }
        }
        return null;
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
        cachedPath = new();
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
    public (Vector3 dir, NavMeshPathStatus status, float pathLength) GetSteeringTo(Vector3 target)
    {
        return NavSteering.GetSteeringDirection(ctx.NavAgent, ctx.Rigidbody.position, target, 0.01f);
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
                NavMesh.CalculatePath(ctx.Rigidbody.position, result, NavMesh.AllAreas, cachedPath);
                if (cachedPath.status == NavMeshPathStatus.PathComplete && Vector3.Distance(ctx.Rigidbody.position, result) > 5f)
                {
                    return true;
                }
                continue;
            }
        }
        result = origin;
        return false;
    }

    public bool IsMoving() {
        return ctx.Rigidbody.linearVelocity.magnitude > 0.1f;
    }

    public void SetAttacking(bool isAttacking) {
        animator.SetBool(AttackingHash, isAttacking);
    }

    public void UpdateAnimParam()
    {
        animator.SetBool(WalkingHash, IsMoving());
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
            health.TakeDamage(dmgCtx);
            ApexLog($"Attacked {col.gameObject.name} for {dmgCtx.amount} damage.");
        }
    }

    public void OnDeathLoseAggro(DamageContext context)
    {
        if (context.victim == PlayerID.Instance.gameObject && context.attacker == gameObject)
        {
            GameStateManager.Instance.attemptSetState(GameStateManager.GameState.PEACEFUL, PlayerID.Instance.gameObject);
            if (stateMachine.CurrentState is ApexChasingState)
            {
                ChasingState.SetTarget(null);
                ChasingState.chasingPlayer = false;
                RoamingState.SetGuardPosition(transform.position);
                stateMachine.ChangeState(RoamingState);
            }
            if (stateMachine.CurrentState is ApexAttackingState)
            {
                RoamingState.SetGuardPosition(transform.position);
                stateMachine.ChangeState(RoamingState);
            }
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

    #region MonoBehaviour Callbacks

    protected override void Update()
    {
        base.Update();
        UpdateAnimParam();
    }

    private void OnEnable()
    {
        EntityHealthManager.OnDeath += OnDeathLoseAggro;
    }

    private void OnDisable()
    {
        EntityHealthManager.OnDeath -= OnDeathLoseAggro;
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

