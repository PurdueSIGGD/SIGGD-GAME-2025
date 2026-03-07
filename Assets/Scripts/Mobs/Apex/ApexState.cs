using Extensions.StateMachine;

public class ApexState : State
{
    protected Apex apex;

    public ApexState(Apex apex)
    {
        this.apex = apex;
    }
}