using SIGGD.Mobs.StateMachine;
using SIGGD.Mobs;
using UnityEngine;

/// <summary>
/// Moves the Apex to a provided investigation position once, then transitions to SearchingState.
/// </summary>
public class ApexInvestigateState : IMobState
{
    private readonly Apex apex;
    private readonly MobContext ctx;

    private Vector3 investigatePosition;
    private bool hasTarget;
    private bool loggedZeroDir;

    private const float NoPathGraceSec = 1f;
    private const float StuckTimeoutSec = 5f;
    private const float StuckMoveThreshold = 0.5f;
    private Vector3 lastProgressPosition;
    private float stuckElapsed;
    private bool waiting;
    private float waitElapsed;
    private float noPathElapsed;

    public ApexInvestigateState(Apex apex)
    {
        this.apex = apex;
        this.ctx = apex.Context;
    }

    /// <summary>
    /// Prepare the state with a world position to investigate.
    /// Call this before changing to this state: e.g. <c>investigateState.SetTarget(pos); stateMachine.ChangeState(investigateState);</c>
    /// </summary>
    public void SetTarget(Vector3 position)
    {
        investigatePosition = position;
        hasTarget = true;
        loggedZeroDir = false;
        waiting = false;
        waitElapsed = 0f;
        noPathElapsed = 0f;
    }

    public void Enter()
    {
        if (!hasTarget)
        {
            apex.ApexLog("InvestigateState - entered without a target, switching to SearchingState.");
            //apex.StateMachine.ChangeState(apex.SearchingState);
            apex.RoamingState.SetGuardPosition(apex.transform.position);
            apex.StateMachine.ChangeState(apex.RoamingState);
            return;
        }

        lastProgressPosition = ctx.Rigidbody.position;
        stuckElapsed = 0f;
        apex.ApexLog($"InvestigateState - moving to investigation point {investigatePosition}.");
    }

    public void Update()
    {
        if (!hasTarget) return;

        if (apex.IsAtPosition(investigatePosition))
        {
            apex.ApexLog("InvestigateState - arrived at investigate point, switching to SearchingState.");
            hasTarget = false;
            apex.RoamingState.SetGuardPosition(apex.transform.position);
            apex.StateMachine.ChangeState(apex.RoamingState);
        }
    }

    public void FixedUpdate()
    {
        if (!hasTarget) return;

        // Standing at the closest spot it could reach: wait a while, then go back to roaming.
        if (waiting)
        {
            waitElapsed += Time.fixedDeltaTime;
            if (waitElapsed >= apex.FindPathBuffer)
            {
                apex.ApexLog("InvestigateState - could not reach investigate point, done waiting, switching to RoamingState.");
                hasTarget = false;
                apex.RoamingState.SetGuardPosition(apex.transform.position);
                apex.StateMachine.ChangeState(apex.RoamingState);
            }
            return;
        }

        var (dir, status, pathLength) = apex.GetSteeringTo(investigatePosition);

        // The investigate point is often somewhere the Apex cannot walk to (its NavMesh is split into
        // regions). Follow the partial path as far as it goes, then wait there.
        if (status == UnityEngine.AI.NavMeshPathStatus.PathPartial && pathLength < 5f)
        {
            apex.ApexLog("InvestigateState - reached the end of a partial path, waiting before roaming.");
            StartWaiting();
            return;
        }

        // No path at all. The first frames after a new destination can report this before the path is
        // ready, so only give up once it has stayed that way.
        if (status == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            noPathElapsed += Time.fixedDeltaTime;
            if (noPathElapsed >= NoPathGraceSec)
            {
                apex.ApexLog("InvestigateState - no path to investigate point, waiting before roaming.");
                StartWaiting();
            }
            return;
        }
        noPathElapsed = 0f;

        ctx.Movement.MoveTowards(dir, apex.ApproachSpeedMulti, 0.1f, false);

        // Not getting anywhere (blocked, or steering has nowhere to go): treat it like an unreachable point.
        if (Vector3.Distance(ctx.Rigidbody.position, lastProgressPosition) > StuckMoveThreshold)
        {
            lastProgressPosition = ctx.Rigidbody.position;
            stuckElapsed = 0f;
        }
        else
        {
            stuckElapsed += Time.fixedDeltaTime;
            if (stuckElapsed >= StuckTimeoutSec)
            {
                apex.ApexLog("InvestigateState - no progress towards investigate point, waiting before roaming.");
                StartWaiting();
            }
        }
    }

    private void StartWaiting()
    {
        waiting = true;
        waitElapsed = 0f;
    }

    public void Exit()
    {
        apex.ApexLog("Exiting InvestigateState.");
        hasTarget = false;
    }

    private bool IsValidDirection(Vector3 d)
    {
        if (float.IsNaN(d.x) || float.IsNaN(d.y) || float.IsNaN(d.z)) return false;
        return d.sqrMagnitude > 0.0001f;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(investigatePosition, 10f);
        Gizmos.DrawLine(apex.transform.position, investigatePosition);

        var (dir, path) = NavSteering.GetSteeringDirection(ctx.NavAgent, apex.transform.position, investigatePosition, 0.01f, true);
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
        }
    }
}