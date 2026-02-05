using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace SIGGD.Mobs
{
    public static class NavSteering
    {
        private static readonly Dictionary<int, Vector3> smoothTargets = new();
        private static readonly Dictionary<int, float> nextUpdate = new();

        // Uses currentPos instead of agent.transform.position
        public static Vector3 GetSteeringDirection(NavMeshAgent agent, Vector3 currentPos, Vector3 destination, float updateRate)
        {
            if (agent == null)
                return Vector3.zero;

            int id = agent.GetInstanceID();

            if (!nextUpdate.ContainsKey(id))
                nextUpdate[id] = 0f;

            agent.nextPosition = currentPos;

            // If the next update for the agent has surpassed the minimum time then set the destination of the agent
            if (Time.time >= nextUpdate[id]) {
                nextUpdate[id] = Time.time + updateRate;
                agent.SetDestination(destination);
            }

            if (!agent.hasPath || agent.path.corners.Length < 2)
                return Vector3.zero;

            Vector3 raw = agent.path.corners[1];

            if (!smoothTargets.ContainsKey(id))
                smoothTargets[id] = raw;
            // Calculates smoothing factor and smooths the stored value to the new value
            float a = 1f - Mathf.Exp(-20f * Time.fixedDeltaTime);
            smoothTargets[id] = Vector3.Lerp(smoothTargets[id], raw, a);

            Vector3 dir = smoothTargets[id] - currentPos;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.0001f)
                return Vector3.zero;

            return dir.normalized;
        }
        public static Vector3 GetSteeringDirection(NavMeshAgent agent, Vector3 destination, float updateRate)
        {
            if (agent == null)
                return Vector3.zero;

            int id = agent.GetInstanceID();

            if (!nextUpdate.ContainsKey(id))
                nextUpdate[id] = 0f;

            // If the next update for the agent has surpassed the minimum time then set the destination of the agent
            if (Time.time >= nextUpdate[id])
            {
                nextUpdate[id] = Time.time + updateRate;
                agent.SetDestination(destination);
            }

            if (!agent.hasPath || agent.path.corners.Length < 2)
                return Vector3.zero;

            Vector3 raw = agent.path.corners[1];

            if (!smoothTargets.ContainsKey(id))
                smoothTargets[id] = raw;
            // Calculates smoothing factor and smooths the stored value to the new value
            float a = 1f - Mathf.Exp(-20f * Time.fixedDeltaTime);
            smoothTargets[id] = Vector3.Lerp(smoothTargets[id], raw, a);

            Vector3 dir = smoothTargets[id] - agent.transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.0001f)
                return Vector3.zero;

            return dir.normalized;
        }
    }

}
