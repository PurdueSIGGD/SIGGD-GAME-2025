using UnityEngine;

namespace SIGGD.Mobs.StateMachine.States
{
    public class ChasePreyState : IMobState
    {
        private readonly MobContext ctx;

        private float lostSightTimer;
        private Transform preyTarget;

        private const float LostSightTimeout = 5f;
        private const float AttackRange = 15f;

        public ChasePreyState(MobContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            ctx.Movement.EnableSprint();
            lostSightTimer = 0f;
            UpdatePreyTarget();
        }

        public void Update()
        {
            UpdatePreyTarget();

            if (preyTarget != null)
                lostSightTimer = 0f;
            else
                lostSightTimer += Time.deltaTime;
        }

        public void FixedUpdate()
        {
            if (preyTarget == null) return;

            Vector3 dir = NavSteering.GetSteeringDirection(
                ctx.NavAgent, ctx.Rigidbody.position, preyTarget.position, 0.1f).dir;
            ctx.Movement.MoveTowards(dir, 1.0f, 3f, true);
        }

        public void Exit()
        {
            ctx.Movement.DisableSprint();
        }

        public bool HasLostPrey => lostSightTimer >= LostSightTimeout;

        public Transform CurrentTarget => preyTarget;

        public bool IsInAttackRange
        {
            get
            {
                if (preyTarget == null) return false;
                float dist = Vector3.Distance(ctx.Rigidbody.position, preyTarget.position);
                return dist <= AttackRange;
            }
        }

        private void UpdatePreyTarget()
        {
            // Keep tracking the current hunt target if it is still alive
            if (ctx.HuntBehaviour != null &&
                ctx.HuntBehaviour.currentTargetOfHunt != null &&
                ctx.HuntBehaviour.currentTargetOfHunt.activeInHierarchy)
            {
                preyTarget = ctx.HuntBehaviour.currentTargetOfHunt.transform;
                return;
            }

            // Pick the closest visible prey from PerceptionManager
            Transform best = null;
            float closest = float.MaxValue;

            if (ctx.Perception != null && ctx.Perception.preyTargets != null)
            {
                foreach (var prey in ctx.Perception.preyTargets)
                {
                    if (prey == null) continue;
                    float d = Vector3.Distance(ctx.Rigidbody.position, prey.transform.position);
                    if (d < closest) { closest = d; best = prey.transform; }
                }
            }

            // Also consider the closest prey detected by Smell
            if (ctx.Smell != null && ctx.Smell.ClosestPrey != null)
            {
                float d = Vector3.Distance(ctx.Rigidbody.position, ctx.Smell.ClosestPrey.position);
                if (d < closest) { closest = d; best = ctx.Smell.ClosestPrey; }
            }

            if (best != null)
            {
                preyTarget = best;
                if (ctx.HuntBehaviour != null)
                    ctx.HuntBehaviour.SetHuntTarget(best.gameObject);
            }
            else
            {
                preyTarget = null;
            }
        }
    }
}
