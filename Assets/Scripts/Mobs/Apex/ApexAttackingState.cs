using UnityEngine;

/// <summary>
/// The Apex is within attack range of a target. It stops moving and triggers
/// a one-shot overlap-sphere attack via <see cref="Apex.DoAttack"/>. After the
/// attack it transitions to <see cref="ApexRoamingState"/> around the position
/// where the kill occurred, matching the specified post-kill roaming behaviour.
/// </summary>
public class ApexAttackingState : ApexState
{
    #region Private State

    private readonly ApexTarget target;
    private readonly Vector3 killPosition;
    private bool hasAttacked;

    #endregion

    /// <param name="target">The target being attacked (used to anchor the post-kill roam position).</param>
    /// <param name="killPosition">World position to roam around after the kill.</param>
    public ApexAttackingState(Apex apex, ApexTarget target, Vector3 killPosition) : base(apex)
    {
        this.target = target;
        this.killPosition = killPosition;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        apex.StopMoving();
        hasAttacked = false;
        apex.ApexLog($"Entering AttackingState — target '{(target != null ? target.gameObject.name : "null")}' at {killPosition}.");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!hasAttacked)
        {
            hasAttacked = true;
            apex.ApexLog("AttackingState — performing attack.");
            apex.DoAttack();
            apex.ApexLog("AttackingState — attack complete, switching to RoamingState.");
            apex.stateController.ChangeState(new ApexRoamingState(apex, killPosition));
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        apex.ApexLog("Exiting AttackingState.");
    }
}