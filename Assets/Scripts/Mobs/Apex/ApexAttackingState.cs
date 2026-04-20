using SIGGD.Mobs.StateMachine;
using UnityEngine;

/// <summary>
/// The Apex is within attack range of a target. It stops and triggers a one-shot
/// overlap-sphere attack via <see cref="Apex.DoAttack"/>. After the attack it
/// transitions to <see cref="ApexRoamingState"/> around the kill position.
/// </summary>
public class ApexAttackingState : IMobState
{
    private readonly Apex apex;

    private ApexTarget target;
    private Vector3 killPosition;
    private bool hasAttacked;

    public ApexAttackingState(Apex apex)
    {
        this.apex = apex;
    }

    /// <summary>Set the attack target and roam-after-kill position before transitioning into this state.</summary>
    public void SetTarget(ApexTarget apexTarget, Vector3 position)
    {
        target = apexTarget;
        killPosition = position;
    }

    public void Enter()
    {
        hasAttacked = false;
        GameStateManager.Instance.attemptSetState(GameStateManager.GameState.PURSUED_BY_APEX, apex.gameObject);
        apex.ApexLog($"Entering AttackingState — target '{(target != null ? target.gameObject.name : "null")}' at {killPosition}.");
    }

    public void Update()
    {
        if (!hasAttacked)
        {
            hasAttacked = true;
            apex.ApexLog("AttackingState — performing attack.");
            apex.DoAttack();
            apex.ApexLog("AttackingState — attack complete, switching to RoamingState.");
            apex.RoamingState.SetGuardPosition(killPosition);
            apex.StateMachine.ChangeState(apex.RoamingState);
        }
    }

    public void FixedUpdate() { }

    public void Exit()
    {
        GameStateManager.Instance.attemptSetState(GameStateManager.GameState.PEACEFUL, apex.gameObject);
        apex.ApexLog("Exiting AttackingState.");
    }
}