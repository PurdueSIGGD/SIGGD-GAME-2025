using SIGGD.Mobs.StateMachine;
using SIGGD.Mobs;
using UnityEngine;
using UnityEditor;

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

    // Fallback: if Apex cannot find a path OR hasn't moved significantly for this duration,
    // abandon roaming and switch to SearchingState.
    private readonly float stuckTimeoutSec = 5f;
    private readonly float moveThreshold = 0.5f;
    private float stuckElapsedSec;
    private Vector3 lastPosition;

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
        if (apex.TryGetRoamPoint(guardPosition, Random.Range(apex.RoamRadius.x, apex.RoamRadius.y), out Vector3 point))
        {
            targetPosition = point;
            hasTarget = true;
            apex.ApexLog($"RoamingState — will move once to waypoint {point}.");

            // initialize fallback trackers
            lastPosition = ctx.Rigidbody != null ? ctx.Rigidbody.position : apex.transform.position;
            stuckElapsedSec = 0f;
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

        Vector3 dir = apex.GetSteeringTo(targetPosition).dir;

        // Guard: avoid commanding movement/rotation on an invalid or near-zero direction.
        if (!IsValidDirection(dir))
        {
            if (!loggedZeroDir)
            {
                apex.ApexLog($"RoamingState.FixedUpdate: invalid or near-zero steering dir for target {targetPosition}. Skipping movement.");
                loggedZeroDir = true;
            }

            // still track being stuck when steering invalid
            stuckElapsedSec += Time.fixedDeltaTime;
            if (stuckElapsedSec >= stuckTimeoutSec)
            {
                apex.ApexLog("RoamingState — no movement detected for timeout, switching to SearchingState.");
                hasTarget = false;
                apex.StateMachine.ChangeState(apex.SearchingState);
            }
            return;
        }

        ctx.Movement.MoveTowards(dir, apex.RoamSpeedMulti, 3f, false);

        // Progress check: consider moved if changed position beyond threshold
        Vector3 currentPos = ctx.Rigidbody != null ? ctx.Rigidbody.position : apex.transform.position;
        float moved = Vector3.Distance(currentPos, lastPosition);
        if (moved > moveThreshold)
        {
            lastPosition = currentPos;
            stuckElapsedSec = 0f;
        }
        else
        {
            stuckElapsedSec += Time.fixedDeltaTime;
            if (stuckElapsedSec >= stuckTimeoutSec)
            {
                apex.ApexLog("RoamingState — insufficient movement for timeout, switching to SearchingState.");
                hasTarget = false;
                apex.StateMachine.ChangeState(apex.SearchingState);
            }
        }
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
            Gizmos.DrawWireSphere(guardPosition, apex.RoamRadius.x);
            Gizmos.DrawWireSphere(guardPosition, apex.RoamRadius.y);
            if (hasTarget)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(targetPosition, 0.3f);
            }

            Gizmos.DrawLine(apex.transform.position, targetPosition);
        }
        Handles.Label(targetPosition, "Apex Roam Target");
#endif
    }
}