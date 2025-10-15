using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Goap.Behaviours
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        private AgentBehaviour agent;
        private ITarget currentTarget;
        private bool shouldMove;
        public NavMeshAgent navMeshAgent;

        private float speed = 500f;

        private void Awake()
        {
            this.agent = this.GetComponent<AgentBehaviour>();
        }

        private void OnEnable()
        {
            this.agent.Events.OnTargetInRange += this.OnTargetInRange;
            this.agent.Events.OnTargetChanged += this.OnTargetChanged;
            this.agent.Events.OnTargetNotInRange += this.TargetNotInRange;
            this.agent.Events.OnTargetLost += this.TargetLost;
        }

        private void OnDisable()
        {
            this.agent.Events.OnTargetInRange -= this.OnTargetInRange;
            this.agent.Events.OnTargetChanged -= this.OnTargetChanged;
            this.agent.Events.OnTargetNotInRange -= this.TargetNotInRange;
            this.agent.Events.OnTargetLost -= this.TargetLost;
        }

        private void TargetLost()
        {
            this.currentTarget = null;
            this.shouldMove = false;
        }

        private void OnTargetInRange(ITarget target)
        {
            this.shouldMove = false;
        }

        private void OnTargetChanged(ITarget target, bool inRange)
        {
            this.currentTarget = target;
            this.shouldMove = !inRange;
        }

        private void TargetNotInRange(ITarget target)
        {
            this.shouldMove = true;
        }

        public void Update()
        {
            if (this.agent.IsPaused)
                return;

            if (!this.shouldMove)
                return;

            if (this.currentTarget == null)
                return;

            // Move the agent along towards their goal position
            MovePartialPath(this.currentTarget.Position, Time.deltaTime * speed);
        }

        private void OnDrawGizmos()
        {
            if (this.currentTarget == null)
                return;

            Gizmos.DrawLine(this.transform.position, this.currentTarget.Position);
        }

        // Function to move along the navmesh path
        void MovePartialPath(Vector3 destination, float distanceToTravel)
        {
            NavMeshPath path = new NavMeshPath();
            if (!navMeshAgent.CalculatePath(destination, path) || path.corners.Length < 2)
            {
                return;
            }
            Vector3 currentPosition = this.transform.position;
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
                    navMeshAgent.SetDestination(partialTarget);
                    return;
                }
            }

            // If we reach here, move to the final corner (distance is longer than full path)
            navMeshAgent.SetDestination(path.corners[^1]);
        }
    }

}