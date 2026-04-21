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
    }

    public void Enter()
    {
        if (!hasTarget)
        {
            apex.ApexLog("InvestigateState - entered without a target, switching to SearchingState.");
            apex.StateMachine.ChangeState(apex.SearchingState);
            return;
        }

        apex.ApexLog($"InvestigateState - moving to investigation point {investigatePosition}.");
    }

    public void Update()
    {
        if (!hasTarget) return;

        if (apex.IsAtPosition(investigatePosition))
        {
            apex.ApexLog("InvestigateState - arrived at investigate point, switching to SearchingState.");
            hasTarget = false;
            apex.StateMachine.ChangeState(apex.SearchingState);
        }
    }

    public void FixedUpdate()
    {
        if (!hasTarget) return;

        var (dir, status, pathLength) = apex.GetSteeringTo(investigatePosition);

        // if status is partial and near the end of path, switch to roaming state
        if (status == UnityEngine.AI.NavMeshPathStatus.PathPartial && pathLength < 5f)
        {
            apex.ApexLog("InvestigateState - path to investigate point is partial and near, switching to RoamingState.");
            hasTarget = false;
            apex.StateMachine.ChangeState(apex.RoamingState);
            return;
        }

        ctx.Movement.MoveTowards(dir, apex.ApproachSpeedMulti, 0.1f, false);
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
    }
}