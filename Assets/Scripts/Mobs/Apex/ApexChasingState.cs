using SIGGD.Mobs.StateMachine;
using SIGGD.Mobs;
using UnityEngine;

/// <summary>
/// The Apex actively chases a visible <see cref="ApexTarget"/>.
/// Once within <see cref="Apex.AttackRange"/> it transitions to
/// <see cref="ApexAttackingState"/>. If the target is destroyed mid-chase
/// the Apex returns to roaming around its last known position.
/// </summary>
public class ApexChasingState : IMobState
{
    private readonly Apex apex;
    private readonly MobContext ctx;

    private ApexTarget target;
    private Vector3 lastKnownPosition;

    public ApexChasingState(Apex apex)
    {
        this.apex = apex;
        this.ctx = apex.Context;
    }

    /// <summary>Set the pursuit target before transitioning into this state.</summary>
    public void SetTarget(ApexTarget apexTarget)
    {
        target = apexTarget;
    }

    public void Enter()
    {
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

    public void Update()
    {
        if (target == null)
        {
            apex.ApexLog($"ChasingState — target lost/destroyed, switching to RoamingState around {lastKnownPosition}.");
            apex.RoamingState.SetGuardPosition(lastKnownPosition);
            apex.StateMachine.ChangeState(apex.RoamingState);
            return;
        }

        lastKnownPosition = target.transform.position;

        float dist = Vector3.Distance(ctx.Rigidbody.position, lastKnownPosition);
        if (dist <= apex.AttackRange)
        {
            apex.ApexLog($"ChasingState — target '{target.gameObject.name}' in attack range, switching to AttackingState.");
            apex.AttackingState.SetTarget(target, lastKnownPosition);
            apex.StateMachine.ChangeState(apex.AttackingState);
        } else if (dist <= apex.LungeRange)
        {
            apex.ApexLog($"ChasingState — target '{target.gameObject.name}' in lunge range, switching to LungingState.");
            apex.LungingState.SetTarget(target, lastKnownPosition);
            apex.StateMachine.ChangeState(apex.LungingState);
        }
    }

    public void FixedUpdate()
    {
        if (target == null) return;

        Vector3 dir = apex.GetSteeringTo(lastKnownPosition);
        ctx.Movement.MoveTowards(dir, apex.ChaseSpeedMulti, 3f, false);
    }

    public void Exit()
    {
        apex.ApexLog("Exiting ChasingState.");
    }
}
