using UnityEngine.AI;
using UnityEngine;

namespace SIGGD.Goap
{
    public class Pathfinding : MonoBehaviour
    {
        // Function to move along the navmesh path for a certain distance (before reanalyzing the path as it still moves)
        public static Vector3 MovePartialPath(NavMeshAgent navMeshAgent, Vector3 destination, float distanceToTravel)
        {
            NavMeshPath path = new NavMeshPath();
            if (!navMeshAgent.CalculatePath(destination, path) || path.corners.Length < 2)
            {
                return Vector3.zero;
            }
            if (path.status != NavMeshPathStatus.PathComplete)
                return Vector3.zero;

        public static Vector3 ERR_VECTOR = new Vector3(-9999999, -9999999, -999999);

        public static Vector3 ShiftTargetToNavMesh(Vector3 rawDest)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(rawDest, out hit, 10f, NavMesh.AllAreas))
            {

                return hit.position;
            }
            Debug.LogError("Could not set a valid move target position on the navmesh!");
            return ERR_VECTOR;
        }
        
        // Function to move along the navmesh path for a certain distance (before reanalyzing the path as it still moves)
        public static void MovePartialPath(NavMeshAgent navMeshAgent, Vector3 _destination, float distanceToTravel)
        {
            Vector3 meshDest = ShiftTargetToNavMesh(_destination);
            if (meshDest == ERR_VECTOR)
            {
                return;
            }
            navMeshAgent.SetDestination(meshDest);
            Debug.Log(meshDest);
            /*NavMeshPath path = new NavMeshPath();
            if (!navMeshAgent.CalculatePath(destination, path) || path.corners.Length < 2)
            {
                return;
            }
            Vector3 currentPosition = navMeshAgent.transform.position;
            float distanceRemaining = distanceToTravel;

            for (int i = 1; i < path.corners.Length; i++)
            {
                Vector3 segmentStart = path.corners[i - 1];
                Vector3 segmentEnd = path.corners[i];
                float segmentLength = Vector3.Distance(segmentStart, segmentEnd);

                if (distanceRemaining > segmentLength)
                {
                    distanceRemaining -= segmentLength;
                }
                else
                {
                    // Move partway through this segment

                    Vector3 direction = (segmentEnd - segmentStart).normalized;
                    Vector3 partialTarget = segmentStart + direction * distanceRemaining;
                    if (NavMesh.SamplePosition(partialTarget, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                        return hit.position;
                    return partialTarget;
                    //navMeshAgent.SetDestination(partialTarget);
                    //return;
                }
            }

            // If we reach here, move to the final corner (distance to travel is longer than full path)
            //navMeshAgent.SetDestination(path.corners[^1]);
            return path.corners[^1];
                    Vector3 direction = (segmentEnd - segmentStart).normalized;
                    Vector3 partialTarget = segmentStart + direction * distanceRemaining;
                    navMeshAgent.SetDestination(partialTarget);
                    return;
                }
            }*/

            // If we reach here, move to the final corner (distance to travel is longer than full path)
            //navMeshAgent.SetDestination(path.corners[^1]);
        }
        public static double GetPathDistance(NavMeshAgent navMeshAgent, Vector3 start, Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();

            // Try to calculate the path between start and end positions
            if (NavMesh.CalculatePath(start, destination, NavMesh.AllAreas, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                float distance = 0f;

                // Add up all the corner-to-corner distances
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }

                return distance;
            }

            // If no valid path could be found
            return Mathf.Infinity;
        }
    }
}