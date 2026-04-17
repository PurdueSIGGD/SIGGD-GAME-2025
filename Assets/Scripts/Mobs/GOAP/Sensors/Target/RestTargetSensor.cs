using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Interfaces;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using SIGGD.Mobs;

namespace SIGGD.Goap.Sensors
{
    public class RestTargetSensor : LocalTargetSensorBase
    {
        private Smell smell;
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var navFilter = references.GetCachedComponent<AgentData>().filter;

            var perceptionManager = references.GetCachedComponent<PerceptionManager>();
            if (perceptionManager == null) return null;

            var smellPos = perceptionManager.GetSmellPosition();
            float safeDistanceThreshold = 30f;
            if (smellPos == Vector3.zero) return null;

            float distance = Vector3.Distance(smellPos, agent.Transform.position);
            if (distance > safeDistanceThreshold) return null;

            Vector3 dirFromPredator = (smellPos - agent.Transform.position).normalized;
            dirFromPredator += dirFromPredator * (1 + Random.Range(-0.1f, 0.1f));
            Vector3 safePosition = agent.Transform.position - dirFromPredator * Mathf.Max(0, safeDistanceThreshold - distance);
            float sampleSphereRadius = 10f;
            Vector3 randomPos;
            int attempts = 10;

            NavMeshPath path = new NavMeshPath();
            for (int i = 0; i < attempts; i++)
            {
                randomPos = safePosition + Random.insideUnitSphere * sampleSphereRadius;
                if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5f, navFilter))
                {
                    if (NavMesh.CalculatePath(agent.Transform.position, hit.position, navFilter, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                    {
                        return new PositionTarget(hit.position);
                    }
                }
                safePosition -= dirFromPredator;
            }
            return null;
        }

        public override void Update()
        {
        }
    }
}
