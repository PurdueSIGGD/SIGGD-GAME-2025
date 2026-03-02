using SIGGD.Mobs.StateMachine;
using UnityEngine;

/// <summary>
/// The Apex moves to a single random roam point (once) around a guard position.
/// When that waypoint is reached the Apex immediately transitions back to
/// <see cref="ApexSearchingState"/>. LOS detection is handled globally by
/// <see cref="Apex.EvaluateTransitions"/>.
/// </summary>
public class ApexRoamingState : IMobState
{
    private readonly Apex apex;
    private readonly MobContext ctx;

    private Vector3 guardPosition;
    private Vector3 targetPosition;
    private bool hasTarget;
    private bool loggedZeroDir = false;

    private const float ArrivalDistance = 2f;

    public ApexRoamingState(Apex apex)
    {
        this.apex = apex;
        this.ctx = apex.Context;
    }

    /// <summary>Set the center of the roam area before transitioning into this state.</summary>
    public void SetGuardPosition(Vector3 position)
    {
        guardPosition = position;
    }

    public void Enter()
    {
        hasTarget = false;
        loggedZeroDir = false;

        // Pick a single roam target on enter. If none found, immediately go back to searching.
        if (apex.TryGetRoamPoint(guardPosition, apex.RoamRadius, out Vector3 point))
        {
            targetPosition = point;
            hasTarget = true;
            apex.ApexLog($"RoamingState — will move once to waypoint {point}.");
        }
        else
        {
            apex.ApexLog("RoamingState — no valid roam point found on NavMesh; switching to SearchingState.");
            apex.StateMachine.ChangeState(apex.SearchingState);
        }
    }

    public void Update()
    {
        // If we have a target and have reached it, transition to searching.
        if (hasTarget && apex.IsAtPosition(targetPosition))
        {
            apex.ApexLog("RoamingState — reached single waypoint, switching to SearchingState.");
            apex.StateMachine.ChangeState(apex.SearchingState);
        }
    }

    public void FixedUpdate()
    {
        if (!hasTarget) return;

        Vector3 dir = apex.GetSteeringTo(targetPosition);

        // Guard: avoid commanding movement/rotation on an invalid or near-zero direction.
        if (!IsValidDirection(dir))
        {
            if (!loggedZeroDir)
            {
                apex.ApexLog($"RoamingState.FixedUpdate: invalid or near-zero steering dir for target {targetPosition}. Skipping movement.");
                loggedZeroDir = true;
            }
            return;
        }

        ctx.Movement.MoveTowards(dir, apex.RoamSpeedMulti, 3f, false);
    }

    public void Exit()
    {
        apex.ApexLog("Exiting RoamingState.");
    }

    private bool IsValidDirection(Vector3 d)
    {
        if (float.IsNaN(d.x) || float.IsNaN(d.y) || float.IsNaN(d.z)) return false;
        return d.sqrMagnitude > 0.0001f;
    }

    public void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (ctx != null && ctx.NavAgent != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(guardPosition, apex.RoamRadius);
            if (hasTarget)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(targetPosition, 0.3f);
            }

            Gizmos.DrawLine(apex.transform.position, targetPosition);
        }
#endif
    }
}