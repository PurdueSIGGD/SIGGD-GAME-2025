using SIGGD.Goap;
using SIGGD.Mobs;
using UnityEngine;
using UnityEngine.AI;

public class ApexHuntLogic : MonoBehaviour
{
    enum State { 
        Attacking, // if player/prey is visible and in range, do an attack. Highest prio if attacking player.
        Hunting, // actively chasing after the player, second highest prio.
        Guarding, // player is dead, but sees mobs within guard radius of player corpse, attacks mobs
        TBagging  // player is dead/not seen, guarding the player's corpse for some time before despawning
    }

    [Header("Attacking")]
    [Tooltip("Visual is the magenta circle around apex")]
    [SerializeField] float attackRange;
    [Tooltip("Currently, the attack is set to one shot, so setting dmg in editor will not matter")]
    [SerializeField] DamageContext attackContext;
    [SerializeField] LayerMask attackLayerMask;

    [Header("Hunting")]
    [Tooltip("Time the apex will pursue the player without line of sight")]
    [SerializeField] float lostSightDuration = 7f;

    [Header("Guarding/Tbagging")]
    [SerializeField] float roamRadius = 12f;
    [SerializeField] float roamInterval = 5f;

    [Header("Despawn Logic")]
    [SerializeField] float timeToDespawn = 30f;
    [SerializeField] MeshRenderer mesh;


    private Movement move;
    private NavMeshAgent agent;
    private PerceptionManager perception;
    private Animator animator;

    private State curState = State.Hunting;
    private GameObject currentTarget;
    private float lostSightTimer;
    private float despawnTimer;

    private bool isAttacking;
    private Vector3 guardLoc;
    private float roamTimer;
    private Vector3 roamTarget;
    private Camera playerCam;

    void Start()
    {
        move = GetComponent<Movement>();
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<PerceptionManager>();
        animator = GetComponent<Animator>();
        roamTarget = transform.position;
        roamTimer = roamInterval * 0.5f;

        playerCam = PlayerID.Instance.cam.GetComponentInChildren<Camera>();
    }

    void FixedUpdate()
    {
        if (!isAttacking) UpdateStates();

        switch (curState)
        {
            case State.Attacking:
                agent.ResetPath();
                isAttacking = true;
                break;
            case State.Hunting:
                guardLoc = currentTarget.transform.position;
                ChaseTarget(currentTarget.transform.position);
                lostSightTimer = 0f;
                despawnTimer = 0f;
                break;
            case State.Guarding:
                if (guardLoc == default) guardLoc = transform.position;
                ChaseTarget(currentTarget.transform.position);
                break;
            case State.TBagging:
                if (guardLoc == default) guardLoc = transform.position;
                RoamBehaviour();
                break;
        }
    }

    private void UpdateStates()
    {
        // if can see player, prio player
        if (perception.CanSeePlayer && perception.PlayerTarget != null)
        {
            currentTarget = perception.PlayerTarget.gameObject;

            if (IsTargetInAttackRange(currentTarget))
            {
                curState = State.Attacking;
                animator.SetTrigger("AttackTrigger");
                return;
            }

            curState = State.Hunting;
            return;
        }

        // if can't see player, but within grace period, still prio player
        lostSightTimer += Time.fixedDeltaTime;
        if (lostSightTimer < lostSightDuration)
        {
            if (currentTarget != null && currentTarget.CompareTag("Player"))
            {
                curState = State.Hunting;
                return;
            }
        }

        // if not chasing player, but close enough to attack mobs, attack
        if (currentTarget != null && IsTargetInAttackRange(currentTarget))
        {
            curState = State.Attacking;
            animator.SetTrigger("AttackTrigger");
            return;
        }

        // chase closest visible mob if possible
        if (perception.preyTargets != null && perception.preyTargets.Count > 0)
        {
            GameObject closest = null;
            var closestDistance = float.MaxValue;

            foreach (var prey in perception.preyTargets) // code here ripped straight from ClosestPreyTargetSensor
            {
                var distance = Vector3.Distance(prey.transform.position, transform.position);

                if (!(distance < closestDistance))
                    continue;

                closest = prey;
                closestDistance = distance;
            }

            currentTarget = closest;
            curState = State.Guarding;
            return;
        }

        despawnTimer += Time.fixedDeltaTime;
        // despawn if not see player for too long, and player don't see it 
        if (curState == State.TBagging && despawnTimer > timeToDespawn && !IsRenderedOnScreen())
        {
            Destroy(gameObject);
        }

        currentTarget = null;
        curState = State.TBagging;
    }

    private void ChaseTarget(Vector3 worldPos)
    {
        Vector3 dir = NavSteering.GetSteeringDirection(agent, worldPos, 0.1f);
        move.MoveTowards(dir, 1f);
    }

    private void RoamBehaviour()
    {
        roamTimer += Time.deltaTime;
        if (roamTimer >= roamInterval || Vector3.Distance(transform.position, roamTarget) < 1)
        {
            roamTarget = PickRandomNavMeshPoint(guardLoc, roamRadius);
            roamTimer = 0f;
            if (roamTarget != Pathfinding.ERR_VECTOR)
                agent.SetDestination(roamTarget);
        }
    }

    private Vector3 PickRandomNavMeshPoint(Vector3 origin, float radius)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = origin + Random.insideUnitSphere * radius;
            randomPos.y = origin.y;
            randomPos = Pathfinding.ShiftTargetToNavMesh(randomPos, 4); // second param should be twice the height of the agent, using hyena's for now
            if (randomPos != Pathfinding.ERR_VECTOR)
                return randomPos;
        }
        return Pathfinding.ERR_VECTOR;
    }

    private bool IsTargetInAttackRange(GameObject other)
    {
        if (other == null) return false;

        float dist = Vector3.Distance(transform.position, other.transform.position);

        if (dist < attackRange) return true;
        return false;
    }

    private bool IsRenderedOnScreen()
    {
        if (!playerCam) return true;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCam);
        return GeometryUtility.TestPlanesAABB(planes, mesh.bounds);
    }

    #region Animation Func

    // a one-shot attack on all entities in range, called from animator
    private void DoAnAttackThingyHere()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, attackLayerMask);
        foreach (Collider col in hits)
        {
            EntityHealthManager health = col.GetComponent<EntityHealthManager>();
            if (health != null && col.gameObject != gameObject)
            {
                attackContext.victim = col.gameObject;
                attackContext.amount = health.MaxHealth;
                health.TakeDamage(attackContext);
            }
        }
    }

    private void ExitAttackBehaviour() // called by the animator to resume updating states
    {
        isAttacking = false;
    }

    #endregion
    #region Gizmos

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(roamTarget, 0.15f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void OnDrawGizmos()
    {
        if (guardLoc != default && curState != State.Hunting)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(guardLoc, roamRadius);
        }
    }
    #endregion
}

