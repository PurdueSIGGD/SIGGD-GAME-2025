using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;
using SIGGD.Goap;
using UnityEngine.Polybrush;
using Utility;

namespace SIGGD.Mobs
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        
        private AgentBehaviour agent;
        private ITarget currentTarget;
        private bool shouldMove;
        private Movement move;
        public NavMeshAgent navMeshAgent;


        [SerializeField] public Transform groundCheckPoint;
        [SerializeField] public Vector3 groundCheckSize = new Vector3(0.49f, 0.3f, 0.49f);
        public LayerMask groundLayer;

        public bool IsGrounded =>
            Physics.CheckBox(groundCheckPoint.position, groundCheckSize, Quaternion.identity, groundLayer);

        private void Awake()
        {
            move = GetComponent<Movement>();
            this.agent = this.GetComponent<AgentBehaviour>();
            this.navMeshAgent = this.GetComponent<NavMeshAgent>();
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
        

        public void FixedUpdate()
        {
            if (this.agent.IsPaused)
                return;

            if (!this.shouldMove)
                return;

            if (this.currentTarget == null)
                return;
            Vector3 desiredDirection = NavSteering.GetSteeringDirection(navMeshAgent, currentTarget.Position, 0.1f);

            move.MoveTowards(desiredDirection, 1.5f);

        }

        private void OnDrawGizmos()
        {
            if (this.currentTarget == null)
                return;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(this.currentTarget.Position, out hit, 10f, NavMesh.AllAreas))
            {
                Gizmos.DrawLine(this.transform.position, hit.position);
            }
            
        }
    }
}