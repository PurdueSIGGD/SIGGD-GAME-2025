using SIGGD.Mobs.StateMachine;
using OneOf;

public class MoggingState : IMobState
{
    private readonly OneOf<Apex, SMHyenaBrain> mob;
    private readonly MobContext ctx;

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

    }
    
    public void Update()
    {

    }

    public void FixedUpdate()
    {

    }
    
    public void Exit()
    {

    }
}
