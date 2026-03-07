using SIGGD.Mobs.StateMachine;
using UnityEngine;

/// <summary>
/// The Apex has reached its alert position and is now scanning for targets.
/// The head bone is swept in an arc via <see cref="ApexLineOfSight"/>.
/// After completing all sweeps without a detection the Apex transitions to
/// <see cref="ApexRoamingState"/>. LOS detection is handled globally by
/// <see cref="Apex.EvaluateTransitions"/>.
/// </summary>
public class ApexSearchingState : IMobState
{
    private readonly Apex apex;

    private float sweepTimer;
    private int sweepsCompleted;
    private int sweepDirection = 1;
    private Quaternion sweepStartRotation;
    private Quaternion sweepEndRotation;

    public ApexSearchingState(Apex apex)
    {
        this.apex = apex;
    }

    public void Enter()
    {
        sweepsCompleted = 0;
        sweepDirection = 1;
        BeginSweep();
        apex.ApexLog($"Entering SearchingState — will perform {apex.SweepsBeforeRoam} sweep(s).");
    }

    public void Update()
    {
        // LOS detection is handled globally by EvaluateTransitions.
        TickSweep();
    }

    public void FixedUpdate() { }

    public void Exit()
    {
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
        apex.ApexLog($"SearchingState — starting sweep {sweepsCompleted + 1}/{apex.SweepsBeforeRoam}, direction {(sweepDirection > 0 ? "right" : "left")}.");
    }

    private Quaternion MakeSweepRotation(float angle)
    {
        return apex.HeadSweepAxis switch
        {
            HeadSweepAxis.X => Quaternion.Euler(angle, 0f, 0f),
            HeadSweepAxis.Z => Quaternion.Euler(0f, 0f, angle),
            _ => Quaternion.Euler(0f, angle, 0f),
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
            apex.RoamingState.SetGuardPosition(apex.Context.Transform.position);
            apex.StateMachine.ChangeState(apex.RoamingState);
            return;
        }

        BeginSweep();
    }

    #endregion
}