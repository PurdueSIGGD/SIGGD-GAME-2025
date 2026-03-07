namespace SIGGD.Mobs.StateMachine
{
    public interface IMobState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }
}
