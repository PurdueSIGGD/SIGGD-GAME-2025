using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs.StateMachine.States
{
    public class WanderState : IMobState
    {
        private readonly MobContext ctx;

        private Vector3 targetPosition;
        private float wanderTimer;
        private float stuckTimer;
        private Vector3 lastPosition;

        private const float WanderRadius = 20f;
        private const float MaxWanderDuration = 5f;
        private const float MinWanderDuration = 3f;
        private const float StuckThreshold = 2f;
        private const float ArrivalDistance = 2f;

        public WanderState(MobContext context)
        {
            ctx = context;
            ctx.Movement.InitializeMobContext(ctx);
        }

        public void Enter()
        {
            PickNewTarget();
            wanderTimer = Random.Range(MinWanderDuration, MaxWanderDuration);
            stuckTimer = 0f;
            lastPosition = ctx.Rigidbody.position;
            ctx.Movement.DisableSprint();
        }

        public void Update()
        {
            wanderTimer -= Time.deltaTime;

            if (Vector3.Distance(ctx.Rigidbody.position, lastPosition) < 0.01f)
                stuckTimer += Time.deltaTime;
            else
                stuckTimer = 0f;

            lastPosition = ctx.Rigidbody.position;

            if (wanderTimer <= 0f || stuckTimer > StuckThreshold ||
                Vector3.Distance(ctx.Rigidbody.position, targetPosition) < ArrivalDistance)
            {
                PickNewTarget();
                wanderTimer = Random.Range(MinWanderDuration, MaxWanderDuration);
                stuckTimer = 0f;
            }
        }

        public void FixedUpdate()
        {
            Vector3 dir = NavSteering.GetSteeringDirection(
                ctx.NavAgent, ctx.Rigidbody.position, targetPosition, 0.1f);
            ctx.Movement.MoveTowards(dir, 1.0f, 3f, false);
        }

        public void Exit() { }

        private void PickNewTarget()
        {
            var boundary = ctx.AgentData.boundary;
            Vector3 randomPos;

            if (boundary != null)
            {
                randomPos = PickWithinBoundary(boundary);
            }
            else
            {
                var offset = Random.insideUnitSphere * WanderRadius;
                offset.y = 0f;
                randomPos = ctx.Rigidbody.position + offset;
            }

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 10f, ctx.AgentData.filter))
                targetPosition = hit.position;
            else
                targetPosition = ctx.Rigidbody.position;
        }

        private Vector3 PickWithinBoundary(Boundary boundary)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 offset = Random.insideUnitCircle * boundary.MaxDist;
                Vector2 query = boundary.Centroid + offset;
                if (boundary.IsInBoundary(query))
                {
                    return new Vector3(query.x, ctx.Rigidbody.position.y, query.y);
                }
            }
            var fallback = Random.insideUnitSphere * WanderRadius;
            fallback.y = 0f;
            return ctx.Rigidbody.position + fallback;
        }
    }
}
