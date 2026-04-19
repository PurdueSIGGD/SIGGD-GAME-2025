using SIGGD.Mobs.StateMachine;
using OneOf;
using UnityEngine;

public class MoggingState : IMobState
{
    private readonly OneOf<Apex, SMHyenaBrain> mob;
    private readonly MobContext ctx;

    private IMobState fallbackState;
    private Transform targetLoc;

    public MoggingState(Apex mob)
    {
        this.mob = mob;
        this.ctx = mob.Context;
    }

    public MoggingState(SMHyenaBrain mob)
    {
        this.mob = mob;
        this.ctx = mob.Context;
    }

    public void Enter()
    {
        // freeze mob movement and return to neutral anim

    }
    
    public void Update()
    {
        // continuously check if path is avaliable to target, if so, return to previous state
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
