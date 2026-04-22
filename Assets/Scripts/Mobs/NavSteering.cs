using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs
{
    public static class NavSteering
    {
        private static readonly Dictionary<int, Vector3> smoothTargets = new();
        private static readonly Dictionary<int, float> nextUpdate = new();

        private static float CalculatePathDistance(Vector3 currentPos, Vector3[] corners)
        {
            if (corners == null || corners.Length < 2) return 0f;
            float distance = Vector3.Distance(currentPos, corners[1]);
            for (int i = 1; i < corners.Length - 1; i++)
            {
                distance += Vector3.Distance(corners[i], corners[i + 1]);
            }
            return distance;
        }

        public static (Vector3 dir, NavMeshPath path) GetSteeringDirection(NavMeshAgent agent, Vector3 currentPos, Vector3 destination, float updateRate, bool whatsup)
        {
            if (agent == null)
                return (Vector3.zero, null);

            int id = agent.GetInstanceID();

            if (!nextUpdate.ContainsKey(id))
                nextUpdate[id] = 0f;

            // Keep the agent aligned with our simulated position
            agent.nextPosition = currentPos;

            // Only set destination at the requested rate to avoid thrashing
            if (Time.time >= nextUpdate[id])
            {
                nextUpdate[id] = Time.time + updateRate;
                if (agent.isOnNavMesh) // guard against agents not on the navmesh
                    agent.SetDestination(destination);
            }

            Vector3 raw = Vector3.zero;
            NavMeshPathStatus currentStatus = NavMeshPathStatus.PathInvalid;
            float pathLength = 0f;

            // If agent has a valid path use the second corner (first steering point)
            if (agent.hasPath && agent.path.corners != null && agent.path.corners.Length >= 2)
            {
                raw = agent.path.corners[1];
                currentStatus = agent.path.status;
                pathLength = CalculatePathDistance(currentPos, agent.path.corners);
            }
            else
            {
                // Try to compute a path directly using NavMesh.CalculatePath as a fallback.
                // This helps when agent.path hasn't been populated yet or agent has updatePosition = false.
                var calcPath = new NavMeshPath();
                bool calcSuccess = NavMesh.CalculatePath(currentPos, destination, NavMesh.AllAreas, calcPath);
                if (calcSuccess && (calcPath.status == NavMeshPathStatus.PathComplete || calcPath.status == NavMeshPathStatus.PathPartial) && calcPath.corners != null && calcPath.corners.Length >= 2)
                {
                    raw = calcPath.corners[1];
                    currentStatus = calcPath.status;
                    pathLength = CalculatePathDistance(currentPos, calcPath.corners);
                }
                else
                {
                    // Last-resort fallback: Use the destination position directly.
                    raw = destination;
                    if (calcSuccess) currentStatus = calcPath.status;
                    pathLength = Vector3.Distance(currentPos, destination);
                }
            }

            // Smooth initialization
            if (!smoothTargets.ContainsKey(id))
                smoothTargets[id] = raw;

            // Calculates smoothing factor and smooths the stored value to the new value
            float a = 1f - Mathf.Exp(-20f * Time.fixedDeltaTime);
            smoothTargets[id] = Vector3.Lerp(smoothTargets[id], raw, a);

            Vector3 dir = smoothTargets[id] - currentPos;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.0001f)
                return (Vector3.zero, agent.path);

            return (dir.normalized, agent.path);
        }

        // Uses currentPos instead of agent.transform.position
        public static (Vector3 dir, NavMeshPathStatus status, float pathLength) GetSteeringDirection(NavMeshAgent agent, Vector3 currentPos, Vector3 destination, float updateRate)
        {
            if (agent == null)
                return (Vector3.zero, NavMeshPathStatus.PathInvalid, 0f);

            int id = agent.GetInstanceID();

            if (!nextUpdate.ContainsKey(id))
                nextUpdate[id] = 0f;

            // Keep the agent aligned with our simulated position
            agent.nextPosition = currentPos;

            // Only set destination at the requested rate to avoid thrashing
            if (Time.time >= nextUpdate[id])
            {
                nextUpdate[id] = Time.time + updateRate;
                if (agent.isOnNavMesh) // guard against agents not on the navmesh
                    agent.SetDestination(destination);
            }

            Vector3 raw = Vector3.zero;
            NavMeshPathStatus currentStatus = NavMeshPathStatus.PathInvalid;
            float pathLength = 0f;

            // If agent has a valid path use the second corner (first steering point)
            if (agent.hasPath && agent.path.corners != null && agent.path.corners.Length >= 2)
            {
                raw = agent.path.corners[1];
                currentStatus = agent.path.status;
                pathLength = CalculatePathDistance(currentPos, agent.path.corners);
            }
            else
            {
                // Try to compute a path directly using NavMesh.CalculatePath as a fallback.
                // This helps when agent.path hasn't been populated yet or agent has updatePosition = false.
                var calcPath = new NavMeshPath();
                bool calcSuccess = NavMesh.CalculatePath(currentPos, destination, NavMesh.AllAreas, calcPath);
                if (calcSuccess && (calcPath.status == NavMeshPathStatus.PathComplete || calcPath.status == NavMeshPathStatus.PathPartial) && calcPath.corners != null && calcPath.corners.Length >= 2)
                {
                    raw = calcPath.corners[1];
                    currentStatus = calcPath.status;
                    pathLength = CalculatePathDistance(currentPos, calcPath.corners);
                }
                else
                {
                    // Last-resort fallback: Use the destination position directly.
                    raw = destination;
                    if (calcSuccess) currentStatus = calcPath.status;
                    pathLength = Vector3.Distance(currentPos, destination);
                }
            }

            // Smooth initialization
            if (!smoothTargets.ContainsKey(id))
                smoothTargets[id] = raw;

            // Calculates smoothing factor and smooths the stored value to the new value
            float a = 1f - Mathf.Exp(-20f * Time.fixedDeltaTime);
            smoothTargets[id] = Vector3.Lerp(smoothTargets[id], raw, a);

            Vector3 dir = smoothTargets[id] - currentPos;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.0001f)
                return (Vector3.zero, currentStatus, pathLength);

            return (dir.normalized, currentStatus, pathLength);
        }

        public static (Vector3 dir, NavMeshPathStatus status, float pathLength) GetSteeringDirection(NavMeshAgent agent, Vector3 destination, float updateRate)
        {
            if (agent == null)
                return (Vector3.zero, NavMeshPathStatus.PathInvalid, 0f);

            int id = agent.GetInstanceID();

            if (!nextUpdate.ContainsKey(id))
                nextUpdate[id] = 0f;

            if (Time.time >= nextUpdate[id])
            {
                nextUpdate[id] = Time.time + updateRate;
                if (agent.isOnNavMesh)
                    agent.SetDestination(destination);
            }

            Vector3 raw = Vector3.zero;
            NavMeshPathStatus currentStatus = NavMeshPathStatus.PathInvalid;
            float pathLength = 0f;

            if (agent.hasPath && agent.path.corners != null && agent.path.corners.Length >= 2)
            {
                raw = agent.path.corners[1];
                currentStatus = agent.path.status;
                pathLength = CalculatePathDistance(agent.transform.position, agent.path.corners);
            }
            else
            {
                var calcPath = new NavMeshPath();
                bool calcSuccess = NavMesh.CalculatePath(agent.transform.position, destination, NavMesh.AllAreas, calcPath);
                if (calcSuccess && (calcPath.status == NavMeshPathStatus.PathComplete || calcPath.status == NavMeshPathStatus.PathPartial) && calcPath.corners != null && calcPath.corners.Length >= 2)
                {
                    raw = calcPath.corners[1];
                    currentStatus = calcPath.status;
                    pathLength = CalculatePathDistance(agent.transform.position, calcPath.corners);
                }
                else
                {
                    raw = destination;
                    if (calcSuccess) currentStatus = calcPath.status;
                    pathLength = Vector3.Distance(agent.transform.position, destination);
                }
            }

            if (!smoothTargets.ContainsKey(id))
                smoothTargets[id] = raw;

            float a = 1f - Mathf.Exp(-20f * Time.fixedDeltaTime);
            smoothTargets[id] = Vector3.Lerp(smoothTargets[id], raw, a);

            Vector3 dir = smoothTargets[id] - agent.transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.0001f)
                return (Vector3.zero, currentStatus, pathLength);

            return (dir.normalized, currentStatus, pathLength);
        }
    }
}
