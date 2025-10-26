using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Interfaces;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Goap.Sensors
{
    public class SafetyTargetSensor : LocalTargetSensorBase
    {
        private Smell smell;
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            smell = references.GetCachedComponent<Smell>();
            float safeDistanceThreshold = 30f;
            var predatorPositionSum = smell.GetSumPredatorPositions();
            if (predatorPositionSum == null) return null;
            float distance = Vector3.Distance(predatorPositionSum, agent.Transform.position);
            if (distance > safeDistanceThreshold) return null;
            smell.positionTest = predatorPositionSum;
            Vector3 dirFromPredator = (predatorPositionSum - agent.Transform.position).normalized;
            dirFromPredator += dirFromPredator * Random.Range(-0.1f, 0.1f);
            Vector3 safePosition = agent.Transform.position - dirFromPredator * Mathf.Max(0, safeDistanceThreshold - distance);
            float sampleSphereRadius = 10f;
            Vector3 randomPos;
            int attempts = 10;
            Debug.Log($"cool{safePosition}");
            for (int i = 0; i < attempts; i++)
            {
                randomPos = safePosition + Random.insideUnitSphere * sampleSphereRadius;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
                {
                    if (existingTarget is PositionTarget positionTarget)
                    {
                        return positionTarget.SetPosition(hit.position);
                    } else
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
