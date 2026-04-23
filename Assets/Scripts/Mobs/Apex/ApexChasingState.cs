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

    public bool chasingPlayer;

    private static readonly string apexLosePlayerSound = "ApexOnLosePlayer";

    public ApexChasingState(Apex apex)
    {
        this.apex = apex;
        this.ctx = apex.Context;
    }

    /// <summary>Set the pursuit target before transitioning into this state.</summary>
    public void SetTarget(ApexTarget apexTarget)
    {
        target = apexTarget;
        if (apexTarget.gameObject == PlayerID.Instance.gameObject) {
            chasingPlayer = true;
            GameStateManager.Instance.attemptSetState(GameStateManager.GameState.PURSUED_BY_APEX, apex.gameObject);
        }
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
            if (chasingPlayer) {
                chasingPlayer = false;
                AudioManager.Instance.PlayOneShotNoAsync(apexLosePlayerSound, PlayerID.Instance.gameObject.transform.position);
            }
            apex.RoamingState.SetGuardPosition(lastKnownPosition);
            apex.StateMachine.ChangeState(apex.RoamingState);
            return;
        }

        lastKnownPosition = target.transform.position;

        if (Vector3.Distance(ctx.Rigidbody.position, lastKnownPosition) <= apex.AttackRange)
        {
            apex.ApexLog($"ChasingState — target '{target.gameObject.name}' in attack range, switching to AttackingState.");
            apex.AttackingState.SetTarget(target, lastKnownPosition);
            apex.StateMachine.ChangeState(apex.AttackingState);
        }
    }

    public void FixedUpdate()
    {
        if (target == null)
        {
            chasingPlayer = false;
            return;
        }

        var (dir, status, pathLength) = apex.GetSteeringTo(lastKnownPosition);
        ctx.Movement.MoveTowards(dir, apex.ChaseSpeedMulti, 3f, false);

        // if near the end of partial path, switching to mogging state
        if (status == UnityEngine.AI.NavMeshPathStatus.PathPartial && pathLength <= 5f)
        {
            apex.ApexLog($"ChasingState — near end of partial path to '{target.gameObject.name}', switching to MoggingState.");
            apex.MoggingState.Configure(this, target.transform);
            apex.StateMachine.ChangeState(apex.MoggingState);
        }
    }

    public void Exit()
    {
        apex.ApexLog("Exiting ChasingState.");
        GameStateManager.Instance.attemptSetState(GameStateManager.GameState.PEACEFUL, apex.gameObject);
    }
}