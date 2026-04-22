using SIGGD.Mobs.StateMachine.States;
using UnityEngine;

namespace SIGGD.Mobs.StateMachine
{
    public class SMPreyBrain : MobBrainBase
    {
        [SerializeField] private Animator animator;

        private FleeState fleeState;

        protected override string MobName => "Prey";

        protected override MobContext BuildContext()
        {
            return new MobContext
            {
                Transform = transform,
                Rigidbody = GetComponent<Rigidbody>(),
                NavAgent = GetComponent<UnityEngine.AI.NavMeshAgent>(),
                Movement = GetComponent<Movement>(),
                AgentData = GetComponent<AgentData>(),
                Pack = GetComponent<PackScripts.PackBehavior>(),
                Perception = GetComponent<PerceptionManager>(),
                Smell = GetComponent<Smell>(),
                Type = MobType.Prey,
                Animator = animator
            };
        }

        protected override void InitializeStates()
        {
            fleeState = new FleeState(ctx);
        }

        protected override void EvaluateTransitions()
        {
            var current = stateMachine.CurrentState;

            // Top priority: flee from predators
            if (InDanger() && current != fleeState)
            {
                stateMachine.ChangeState(fleeState);
                return;
            }

            // Fleeing — wait until safe
            if (current == fleeState)
            {
                if (fleeState.IsSafe)
                    stateMachine.ChangeState(wanderState);
                return;
            }

            // Follow pack when idle
            if (current == wanderState && HasPackToFollow())
            {
                stateMachine.ChangeState(followPackState);
                return;
            }

            // Pack lost alpha
            if (current == followPackState && !followPackState.HasValidAlpha)
            {
                stateMachine.ChangeState(wanderState);
                return;
            }
        }

        private bool InDanger()
        {
            return (ctx.Perception != null && ctx.Perception.predatorTargets.Count > 0) ||
                   (ctx.Smell != null && ctx.Smell.ClosestPred != null);
        }
    }
}
