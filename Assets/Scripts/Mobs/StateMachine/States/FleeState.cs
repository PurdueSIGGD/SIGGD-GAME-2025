using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs.StateMachine.States
{
    public class FleeState : IMobState
    {
        private readonly MobContext ctx;

        private Vector3 fleeTarget;
        private float retargetTimer;
        private float safeTimer;

        private const float FleeRadius = 20f;
        private const float RetargetInterval = 2f;
        private const float SafeDelay = 3f;

        public FleeState(MobContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            ctx.Movement.EnableSprint();
            safeTimer = 0f;
            retargetTimer = 0f;
            PickFleeTarget();
        }

        public void Update()
        {
            retargetTimer -= Time.deltaTime;
            if (retargetTimer <= 0f)
            {
                PickFleeTarget();
                retargetTimer = RetargetInterval;
            }

            bool stillInDanger = ctx.Perception != null && ctx.Perception.predatorTargets.Count > 0;
            if (!stillInDanger)
            {
                safeTimer += Time.deltaTime;
            }
            else
            {
                safeTimer = 0f;
            }
        }

        public void FixedUpdate()
        {
            Vector3 dir = NavSteering.GetSteeringDirection(
                ctx.NavAgent, ctx.Rigidbody.position, fleeTarget, 0.1f).dir;
            ctx.Movement.MoveTowards(dir, 1.0f, 3f, true);
        }

        public void Exit()
        {
            ctx.Movement.DisableSprint();
        }

        public bool IsSafe => safeTimer >= SafeDelay;

        private void PickFleeTarget()
        {
            Vector3 awayDir = GetAwayFromPredatorsDir();

            // Add some randomness so the path isn't perfectly predictable
            Vector3 jitter = Random.insideUnitSphere * 0.3f;
            jitter.y = 0f;
            Vector3 fleeDir = (awayDir + jitter).normalized * FleeRadius;

            Vector3 startPos = ctx.Rigidbody.position;
            // Ensure raycast starts safely on the NavMesh
            if (NavMesh.SamplePosition(startPos, out NavMeshHit startHit, 2f, ctx.AgentData.filter))
            {
                startPos = startHit.position;
            }

            Vector3 candidate = startPos + fleeDir;

            if (NavMesh.Raycast(startPos, candidate, out NavMeshHit hit, ctx.AgentData.filter))
            {
                fleeTarget = hit.position;
            }
            else
            {
                fleeTarget = candidate;
            }
        }

        private Vector3 GetAwayFromPredatorsDir()
        {
            if (ctx.Perception == null || ctx.Perception.predatorTargets.Count == 0)
            {
                Vector3 rand = Random.insideUnitSphere;
                rand.y = 0f;
                return rand.normalized;
            }

            // Average direction away from all visible predators
            Vector3 awaySum = Vector3.zero;
            foreach (var predator in ctx.Perception.predatorTargets)
            {
                if (predator == null) continue;
                Vector3 away = ctx.Rigidbody.position - predator.transform.position;
                away.y = 0f;
                if (away.sqrMagnitude > 0.001f)
                    awaySum += away.normalized;
            }

            if (awaySum.sqrMagnitude < 0.001f)
            {
                Vector3 rand = Random.insideUnitSphere;
                rand.y = 0f;
                return rand.normalized;
            }

            return awaySum.normalized;
        }
    }
}
