using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace SIGGD.Mobs.StateMachine.States
{
    public class AttackPlayerState : IMobState
    {
        private readonly MobContext ctx;

        public AttackPlayerState(MobContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            ctx.Movement.EnableSprint();

            if (ctx.AttackManager == null || ctx.AttackManager.isLunging) return;

            Transform player = ctx.Perception != null ? ctx.Perception.PlayerTarget : null;
            if (player == null) return;

            ctx.AttackManager.SetTarget(new TransformTarget(player));
            ctx.AttackManager.StartAttackSequence(null);
        }

        public void Update() { }

        public void FixedUpdate() { }

        public void Exit()
        {
            ctx.Movement.DisableSprint();
        }

        public bool IsAttackFinished =>
            ctx.AttackManager == null || !ctx.AttackManager.isLunging;
    }
}
