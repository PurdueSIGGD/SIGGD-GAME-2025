using SIGGD.Mobs.StateMachine;
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

        Vector3 dir = apex.GetSteeringTo(investigatePosition);

        // Defensive: avoid commanding movement on invalid/near-zero directions
        if (!IsValidDirection(dir))
        {
            if (!loggedZeroDir)
            {
                apex.ApexLog($"InvestigateState.FixedUpdate: invalid steering dir toward {investigatePosition}. Skipping movement this frame.");
                loggedZeroDir = true;
            }
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