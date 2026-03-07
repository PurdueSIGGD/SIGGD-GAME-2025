using SIGGD.Mobs.StateMachine;
using UnityEngine;

/// <summary>
/// The Apex moves toward the alert position it was spawned with.
/// Once it arrives it transitions to <see cref="ApexSearchingState"/>.
/// LOS detection during approach is handled globally by <see cref="Apex.EvaluateTransitions"/>.
/// </summary>
public class ApexApproachingState : IMobState
{
    private readonly Apex apex;
    private readonly MobContext ctx;

    public ApexApproachingState(Apex apex)
    {
        this.apex = apex;
        this.ctx = apex.Context;
    }

    public void Enter()
    {
        apex.ApexLog($"Entering ApproachingState — moving to alert position {apex.TargetPosition}.");
    }

    public void Update()
    {
        if (apex.IsAtPosition(apex.TargetPosition))
        {
            apex.ApexLog("ApproachingState — reached alert position, switching to SearchingState.");
            apex.StateMachine.ChangeState(apex.SearchingState);
        }
    }

    public void FixedUpdate()
    {
        Vector3 dir = apex.GetSteeringTo(apex.TargetPosition);
        ctx.Movement.MoveTowards(dir, apex.ApproachSpeedMulti, 3f, false);
    }

    public void Exit()
    {
        apex.ApexLog("Exiting ApproachingState.");
    }
}