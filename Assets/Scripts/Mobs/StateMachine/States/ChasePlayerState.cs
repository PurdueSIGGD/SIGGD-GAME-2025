using UnityEngine;

namespace SIGGD.Mobs.StateMachine.States
{
    public class ChasePlayerState : IMobState
    {
        private readonly MobContext ctx;

        private float lostSightTimer;

        private const float LostSightTimeout = 5f;
        private const float AttackRange = 15f;

        public ChasePlayerState(MobContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            ctx.Movement.EnableSprint();
            lostSightTimer = 0f;
        }

        public void Update()
        {
            if (ctx.Perception == null && ctx.Smell == null) return;

            bool canSee = ctx.Perception != null &&
                          ctx.Perception.CanSeePlayer &&
                          ctx.Perception.PlayerTarget != null;
            bool canSmell = ctx.Smell != null && ctx.Smell.PlayerTarget != null;

            if (canSee || canSmell)
                lostSightTimer = 0f;
            else
                lostSightTimer += Time.deltaTime;
        }

        public void FixedUpdate()
        {
            // Prefer the vision target; fall back to the smell target
            Transform player = null;
            if (ctx.Perception != null && ctx.Perception.PlayerTarget != null)
                player = ctx.Perception.PlayerTarget;
            else if (ctx.Smell != null && ctx.Smell.PlayerTarget != null)
                player = ctx.Smell.PlayerTarget;

            if (player == null) return;

            Vector3 dir = NavSteering.GetSteeringDirection(
                ctx.NavAgent, ctx.Rigidbody.position, player.position, 0.1f).dir;
            ctx.Movement.MoveTowards(dir, 1.0f, 3f, true);
        }

        public void Exit()
        {
            ctx.Movement.DisableSprint();
        }

        public bool HasLostPlayer => lostSightTimer >= LostSightTimeout;

        public bool IsInAttackRange
        {
            get
            {
                Transform player = null;
                if (ctx.Perception != null && ctx.Perception.PlayerTarget != null)
                    player = ctx.Perception.PlayerTarget;
                else if (ctx.Smell != null && ctx.Smell.PlayerTarget != null)
                    player = ctx.Smell.PlayerTarget;

                if (player == null) return false;
                float dist = Vector3.Distance(ctx.Rigidbody.position, player.position);
                return dist <= AttackRange;
            }
        }
    }
}
