using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace SIGGD.Mobs.StateMachine.States
{
    public class AttackPreyState : IMobState
    {
        private readonly MobContext ctx;
        private Transform preyTarget;

        public AttackPreyState(MobContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            ctx.Movement.EnableSprint();

            if (ctx.AttackManager == null || ctx.AttackManager.isLunging) return;

            // Use the hunt target tracked by the chase state
            if (ctx.HuntBehaviour != null &&
                ctx.HuntBehaviour.currentTargetOfHunt != null &&
                ctx.HuntBehaviour.currentTargetOfHunt.activeInHierarchy)
            {
                preyTarget = ctx.HuntBehaviour.currentTargetOfHunt.transform;
            }

            if (preyTarget == null) return;

            ctx.AttackManager.SetTarget(new TransformTarget(preyTarget));
            ctx.AttackManager.StartAttackSequence(null);
        }

        public void Update() { }

        public void FixedUpdate() { }

        public void Exit()
        {
            ctx.Movement.DisableSprint();
        }

        public void SetTarget(Transform target)
        {
            preyTarget = target;
        }

        public bool IsAttackFinished =>
            ctx.AttackManager == null || !ctx.AttackManager.isLunging;
    }
}
