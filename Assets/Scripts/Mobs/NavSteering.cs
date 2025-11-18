using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace SIGGD.Mobs
{
    public static class NavSteering
    {
        private static readonly Dictionary<int, Vector3> smoothTargets = new();
        private static readonly Dictionary<int, float> nextUpdate = new();

        private const float smoothFactor = 0.05f;

        public static Vector3 GetSteeringDirection(NavMeshAgent agent, Vector3 destination, float updateRate)
        {
            if (agent == null)
                return Vector3.zero;

            int id = agent.GetInstanceID();

            if (!nextUpdate.ContainsKey(id))
                nextUpdate[id] = 0f;

            if (Time.time >= nextUpdate[id]) {
                nextUpdate[id] = Time.time + updateRate;
                agent.SetDestination(destination);
            }

            if (!agent.hasPath || agent.path.corners.Length < 2)
                return Vector3.zero;

            Vector3 raw = agent.steeringTarget;

            if (!smoothTargets.ContainsKey(id))
                smoothTargets[id] = raw;

            smoothTargets[id] = Vector3.Lerp(smoothTargets[id], raw, smoothFactor);
            Vector3 dir = smoothTargets[id] - agent.transform.position;
            dir.y = 0;
            return dir.normalized;
        }
    }

}