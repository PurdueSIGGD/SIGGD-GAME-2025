using SIGGD.Mobs.StateMachine;
using OneOf;
using UnityEngine;
using UnityEngine.AI;

public class MoggingState : IMobState
{
    private readonly OneOf<Apex, SMHyenaBrain> mob;
    private readonly MobContext ctx;
    private readonly float waitTime;
    private Transform pursueTarget;

    private float timeWaited = 0f;
    private NavMeshPath path;

    public MoggingState(Apex mob, float waitTime)
    {
        this.mob = mob;
        this.ctx = mob.Context;
        this.waitTime = waitTime;
    }

    public MoggingState(SMHyenaBrain mob, float waitTime)
    {
        this.mob = mob;
        this.ctx = mob.Context;
        this.waitTime = waitTime;
    }

    public void SetMogTarget(Transform target)
    {
        pursueTarget = target;
    }

    public void Enter()
    {
        ctx.Animator.SetTrigger("Return to Idle");
        ctx.Rigidbody.linearVelocity = Vector3.zero;
        path = new NavMeshPath();
    }
    
    public void Update()
    {
        // continuously check if path is avaliable to target, if so, return to previous state
        if (timeWaited < waitTime)
        {
            timeWaited += waitTime;

            // recalculate path
            if (pursueTarget != null)
            {
                NavMesh.CalculatePath(ctx.Transform.position, pursueTarget.position, NavMesh.AllAreas, path);
                if (path != null && path.status == NavMeshPathStatus.PathComplete)
                {
                    if (IsApex())
                    {
                        Apex apex = mob.AsT0;
                        apex.ChasingState.SetTarget(pursueTarget.GetComponent<ApexTarget>());
                        apex.StateMachine.ChangeState(apex.ChasingState);
                    }
                    else if (IsHyena())
                    {
                        SMHyenaBrain hyena = mob.AsT1;
                        hyena.StateMachine.ChangeState(hyena.ChasePlayer);
                    }
                    else
                    {
                        Debug.LogError("Mob should only be apex or hyena, tf is this? " + mob);
                    }
                }
            }
        }
        else
        {
            if (IsApex())
            {
                Apex apex = mob.AsT0;
                apex.StateMachine.ChangeState(apex.RoamingState);
            }
            else if (IsHyena())
            {
                SMHyenaBrain hyena = mob.AsT1;
                hyena.StateMachine.ChangeState(hyena.Wander);
            }
            else
            {
                Debug.LogError("Mob should only be apex or hyena, tf is this? " + mob);
            }

            //if (IsApex())
            //{
            //    var apex = mob.AsT0;
            //    pursueTarget = apex.LastKnownTarget;
            //    if (apex.CanPursue(pursueTarget))
            //    {
            //        apex.ChasingState.SetTarget(pursueTarget.GetComponent<ApexTarget>());
            //        apex.StateMachine.ChangeState(apex.ChasingState);
            //    }
            //}
            //else if (IsHyena())
            //{
            //    var hyena = mob.AsT1;
            //    pursueTarget = hyena.LastKnownTarget;
            //    if (hyena.CanPursue(pursueTarget))
            //    {
            //        hyena.ChasingState.SetTarget(pursueTarget.GetComponent<HyenaTarget>());
            //        hyena.StateMachine.ChangeState(hyena.ChasingState);
            //    }
            //}
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

    private bool IsApex()
    {
        return mob.IsT0;
    }

    private bool IsHyena()
    {
        return mob.IsT1;
    }

    #endregion
}
