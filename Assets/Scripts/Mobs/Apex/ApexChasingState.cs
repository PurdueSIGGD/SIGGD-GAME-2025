using UnityEngine;

/// <summary>
/// The Apex actively chases a visible <see cref="ApexTarget"/>.
/// Once within <see cref="Apex.AttackRange"/> it transitions to
/// <see cref="ApexAttackingState"/>. If the target is destroyed mid-chase
/// the Apex returns to roaming around its last known position.
/// </summary>
public class ApexChasingState : ApexState
{
    #region Private State

    private ApexTarget target;
    private Vector3 lastKnownPosition;

    #endregion

    /// <param name="apexTarget">The target the Apex is pursuing.</param>
    public ApexChasingState(Apex apex, ApexTarget apexTarget) : base(apex)
    {
        target = apexTarget;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        if (target != null)
        {
            lastKnownPosition = target.transform.position;
            apex.ApexLog($"Entering ChasingState — pursuing '{target.gameObject.name}' at {lastKnownPosition}.");
        }
        else
        {
            apex.ApexLog("Entering ChasingState — target is already null on enter.");
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (target == null)
        {
            apex.ApexLog($"ChasingState — target lost/destroyed, switching to RoamingState around last known position {lastKnownPosition}.");
            apex.stateController.ChangeState(new ApexRoamingState(apex, lastKnownPosition));
            return;
        }

        lastKnownPosition = target.transform.position;
        apex.ChaseTarget(lastKnownPosition);

        if (IsInAttackRange())
        {
            apex.ApexLog($"ChasingState — target '{target.gameObject.name}' in attack range, switching to AttackingState.");
            apex.stateController.ChangeState(new ApexAttackingState(apex, target, lastKnownPosition));
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        apex.StopMoving();
        apex.ApexLog("Exiting ChasingState.");
    }

    private bool IsInAttackRange()
    {
        return Vector3.Distance(apex.transform.position, lastKnownPosition) <= apex.AttackRange;
    }
}