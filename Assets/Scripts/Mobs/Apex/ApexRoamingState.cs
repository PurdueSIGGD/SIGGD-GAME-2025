using UnityEngine;

/// <summary>
/// The Apex wanders randomly around a guard position, pausing at each waypoint.
/// After <see cref="Apex.RoamDuration"/> seconds it transitions back to
/// <see cref="ApexSearchingState"/> to check for targets again.
/// Any target spotted via <see cref="ApexLineOfSight"/> while roaming immediately
/// transitions to <see cref="ApexChasingState"/>.
/// </summary>
public class ApexRoamingState : ApexState
{
    #region Private State

    private readonly Vector3 guardPosition;
    private float roamTimer;
    private float pauseTimer;
    private bool isPaused;

    #endregion

    /// <param name="guardPosition">The center of the roam area (typically where the Apex last searched).</param>
    public ApexRoamingState(Apex apex, Vector3 guardPosition) : base(apex)
    {
        this.guardPosition = guardPosition;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        roamTimer = 0f;
        isPaused = false;
        MoveToNextRoamPoint();
        apex.ApexLog($"Entering RoamingState — guard position {guardPosition}, duration {apex.RoamDuration}s.");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Guard against missing LOS component.
        if (apex.LineOfSight != null)
        {
            ApexTarget target = apex.LineOfSight.VisibleTarget;
            if (target != null)
            {
                apex.ApexLog($"RoamingState — spotted target '{target.gameObject.name}' while roaming, switching to ChasingState.");
                apex.stateController.ChangeState(new ApexChasingState(apex, target));
                return;
            }
        }

        roamTimer += Time.deltaTime;
        if (roamTimer >= apex.RoamDuration)
        {
            apex.ApexLog("RoamingState — roam duration elapsed, switching to SearchingState.");
            apex.stateController.ChangeState(new ApexSearchingState(apex));
            return;
        }

        if (isPaused)
        {
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= apex.RoamPauseDuration)
            {
                isPaused = false;
                MoveToNextRoamPoint();
            }
        }
        else if (apex.IsAtTarget())
        {
            isPaused = true;
            pauseTimer = 0f;
            apex.StopMoving();
            apex.ApexLog("RoamingState — reached waypoint, pausing.");
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        apex.StopMoving();
        apex.ApexLog("Exiting RoamingState.");
    }

    private void MoveToNextRoamPoint()
    {
        if (apex.TryGetRoamPoint(guardPosition, apex.RoamRadius, out Vector3 point))
        {
            apex.RoamTowardTarget(point);
            apex.ApexLog($"RoamingState — moving to next waypoint {point}.");
        }
        else
        {
            apex.ApexLog("RoamingState — could not find a valid roam point on NavMesh.");
        }
    }
}