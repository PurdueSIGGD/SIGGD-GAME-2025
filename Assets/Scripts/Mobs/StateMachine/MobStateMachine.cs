namespace SIGGD.Mobs.StateMachine
{
    public class MobStateMachine
    {
        public IMobState CurrentState { get; private set; }

        public void ChangeState(IMobState newState)
        {
            if (newState == CurrentState) return;
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Update()
        {
            CurrentState?.Update();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }
    }
}
