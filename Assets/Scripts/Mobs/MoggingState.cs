using SIGGD.Mobs.StateMachine;
using OneOf;
using UnityEngine;
using UnityEngine.AI;
using SIGGD.Mobs.StateMachine.States;

public class MoggingState : IMobState
{
    private readonly MobBrainBase mob;
    private readonly MobContext ctx;
    private readonly float waitTime;
    private IMobState fallbackState;
    private OneOf<Transform, Vector3> possibleTarget;

    private float timeWaited = 0f;
    private NavMeshPath path;

    public MoggingState(MobBrainBase mob, float waitTime)
    {
        this.mob = mob;
        this.ctx = mob.Context;
        this.waitTime = waitTime;
    }

    public void Configure(IMobState fallbackState, Transform target)
    {
        this.fallbackState = fallbackState;
        this.possibleTarget = target;
    }

    public void Configure(IMobState fallbackState, Vector3 target)
    {
        this.fallbackState = fallbackState;
        this.possibleTarget = target;
    }

    public void Enter()
    {
        ctx.Animator.SetTrigger("Return to Idle");
        ctx.Rigidbody.linearVelocity = Vector3.zero;
        path = new NavMeshPath();
    }
    
    public void Update()
    {
        // continuously check if path is available to target, if so, return to previous state
        if (timeWaited < waitTime)
        {
            timeWaited += waitTime;

            // recalculate path
            if (possibleTarget.IsT0)
            {
                NavMesh.CalculatePath(ctx.Transform.position, possibleTarget.AsT0.position, NavMesh.AllAreas, path);
                if (path != null && path.status == NavMeshPathStatus.PathComplete)
                {
                    HandleFallback();
                }
            }
            else if (possibleTarget.IsT1)
            {
                NavMesh.CalculatePath(ctx.Transform.position, possibleTarget.AsT1, NavMesh.AllAreas, path);
                if (path != null && path.status == NavMeshPathStatus.PathComplete)
                {
                    HandleFallback();
                }
            }
        }
        else
        {
            if (mob is Apex apex)
            {
                apex = mob as Apex;
                apex.StateMachine.ChangeState(apex.RoamingState);
            }
            else
            {
                mob.StateMachine.ChangeState(mob.WanderState);
            }
        }
    }

    public void FixedUpdate()
    {

    }
    
    public void Exit()
    {
        Debug.Log($"{ctx.Transform.name} exiting from mogging state");
    }

    #region Helper Methods

    private void HandleFallback()
    {
        Apex apex;
        SMHyenaBrain hyena;

        if (fallbackState != null)
        {
            if (fallbackState is ApexChasingState)
            {
                apex = mob as Apex;
                if (possibleTarget.IsT0)
                {
                    apex.ChasingState.SetTarget(possibleTarget.AsT0.GetComponent<ApexTarget>());
                }
                else
                {
                    apex.ChasingState.SetTarget(null);
                }
                apex.StateMachine.ChangeState(apex.ChasingState);
                return;
            }
            if (fallbackState is ChasePlayerState || fallbackState is AttackPlayerState)
            {
                hyena = mob as SMHyenaBrain;
                hyena.StateMachine.ChangeState(hyena.ChasePlayer);
                return;
            }
            if (fallbackState is ChasePreyState || fallbackState is AttackPreyState)
            {
                hyena = mob as SMHyenaBrain;
                hyena.StateMachine.ChangeState(hyena.ChasePrey);
                return;
            }
        }
        else
        {
            Debug.LogError("MoggingState has no fallback state set!");
        }

        Debug.LogError($"Fell through when transitioning out of MoggingState. {mob} entered mog from {fallbackState}");

        if (mob is Apex)
        {
            apex = mob as Apex;
            apex.StateMachine.ChangeState(apex.RoamingState);
        }
        else
        {
            mob.StateMachine.ChangeState(mob.WanderState);
        }
    }
    #endregion
}
