/// <summary>
/// The Apex moves toward the alert position it was spawned with.
/// Once it arrives it transitions to <see cref="ApexSearchingState"/> and
/// begins scanning its surroundings.
/// </summary>
public class ApexApproachingState : ApexState
{
    public ApexApproachingState(Apex apex) : base(apex) { }

    public override void OnEnter()
    {
        base.OnEnter();
        apex.MoveTowardTarget(apex.TargetPosition);
        apex.ApexLog($"Entering ApproachingState — moving to alert position {apex.TargetPosition}.");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (apex.IsAtTarget())
        {
            apex.ApexLog("ApproachingState — reached alert position, switching to SearchingState.");
            apex.stateController.ChangeState(new ApexSearchingState(apex));
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        apex.StopMoving();
        apex.ApexLog("Exiting ApproachingState.");
    }
}