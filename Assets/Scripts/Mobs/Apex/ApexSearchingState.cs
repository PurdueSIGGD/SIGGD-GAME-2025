using UnityEngine;

/// <summary>
/// The Apex has reached its alert position and is now actively scanning for targets.
/// The <see cref="ApexLineOfSight"/> transform is rotated in a sweeping arc; the
/// head bone follows it automatically via <see cref="ApexLineOfSight"/>.
/// After completing <see cref="Apex.SweepsBeforeRoam"/> sweeps without finding
/// anything the Apex transitions to <see cref="ApexRoamingState"/>. A target spotted
/// mid-sweep immediately triggers <see cref="ApexChasingState"/>.
/// </summary>
public class ApexSearchingState : ApexState
{
    #region Private State

    private float sweepTimer;
    private int sweepsCompleted;
    private int sweepDirection = 1; // 1 = sweeping right, -1 = sweeping left
    private Quaternion sweepStartRotation;
    private Quaternion sweepEndRotation;

    #endregion

    public ApexSearchingState(Apex apex) : base(apex) { }

    public override void OnEnter()
    {
        base.OnEnter();
        apex.StopMoving();
        BeginSweep();
        apex.ApexLog($"Entering SearchingState — will perform {apex.SweepsBeforeRoam} sweep(s).");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Guard against missing LOS component — skip detection but keep sweeping.
        if (apex.LineOfSight != null)
        {
            ApexTarget target = apex.LineOfSight.VisibleTarget;
            if (target != null)
            {
                apex.ApexLog($"SearchingState — spotted target '{target.gameObject.name}', switching to ChasingState.");
                apex.stateController.ChangeState(new ApexChasingState(apex, target));
                return;
            }
        }

        TickSweep();
    }

    public override void OnExit()
    {
        base.OnExit();
        // Reset the LOS (and therefore the head bone) back to neutral on exit.
        apex.LineOfSight?.ResetRotation();
        apex.ApexLog("Exiting SearchingState.");
    }

    #region Sweep

    private void BeginSweep()
    {
        sweepTimer = 0f;
        if (apex.LineOfSight == null) return;

        float halfAngle = apex.HeadSweepAngle * 0.5f;
        sweepStartRotation = MakeSweepRotation(-halfAngle * sweepDirection);
        sweepEndRotation = MakeSweepRotation(halfAngle * sweepDirection);
        apex.LineOfSight.SetLocalRotation(sweepStartRotation);
        apex.ApexLog($"SearchingState — starting sweep {sweepsCompleted + 1}/{apex.SweepsBeforeRoam}, direction {(sweepDirection > 0 ? "right" : "left")} on axis {apex.HeadSweepAxis}.");
    }

    /// <summary>Builds a local-space rotation of <paramref name="angle"/> degrees on the inspector-selected axis.</summary>
    private Quaternion MakeSweepRotation(float angle)
    {
        return apex.HeadSweepAxis switch
        {
            HeadSweepAxis.X => Quaternion.Euler(angle, 0f, 0f),
            HeadSweepAxis.Z => Quaternion.Euler(0f, 0f, angle),
            _ => Quaternion.Euler(0f, angle, 0f), // Y is default
        };
    }

    private void TickSweep()
    {
        if (apex.LineOfSight == null)
        {
            CompleteSweep();
            return;
        }

        sweepTimer += Time.deltaTime;
        float t = Mathf.Clamp01(sweepTimer / apex.HeadSweepDuration);
        apex.LineOfSight.SetLocalRotation(Quaternion.Slerp(sweepStartRotation, sweepEndRotation, t));

        if (t >= 1f)
            CompleteSweep();
    }

    private void CompleteSweep()
    {
        sweepsCompleted++;
        sweepDirection = -sweepDirection;
        apex.ApexLog($"SearchingState — completed sweep {sweepsCompleted}/{apex.SweepsBeforeRoam}.");

        if (sweepsCompleted >= apex.SweepsBeforeRoam)
        {
            apex.ApexLog("SearchingState — no target found after all sweeps, switching to RoamingState.");
            apex.stateController.ChangeState(new ApexRoamingState(apex, apex.transform.position));
            return;
        }

        BeginSweep();
    }

    #endregion
}