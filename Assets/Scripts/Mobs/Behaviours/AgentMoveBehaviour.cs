using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Goap.Behaviours
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        
        private AgentBehaviour agent;
        private AgentSprintBehaviour sprint;
        private ITarget currentTarget;
        private bool shouldMove;
        private bool sprintAllowed;


        public NavMeshAgent navMeshAgent;
        

        public float speed;

        [SerializeField] public Transform groundCheckPoint;
        [SerializeField] public Vector3 groundCheckSize = new Vector3(0.49f, 0.3f, 0.49f);
        public LayerMask groundLayer;

        public bool IsGrounded =>
            Physics.CheckBox(groundCheckPoint.position, groundCheckSize, Quaternion.identity, groundLayer);

        private void Awake()
        {
            this.agent = this.GetComponent<AgentBehaviour>();
            sprint = GetComponent<AgentSprintBehaviour>();
            sprintAllowed = false;
            speed = 3f;
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
            //speed = 1f;
            /*
            if (sprintAllowed)
            {
                if (sprint.stamina > 0)
                {
                    speed = 2f;
                    sprint.stamina -= 8 * Time.deltaTime;
                }
            }
            */
           // this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(this.currentTarget.Position.x, this.transform.position.y, this.currentTarget.Position.z), Time.deltaTime * speed);
           // this.transform.LookAt(currentTarget.Position);

            // Move the agent along towards their goal position
            Pathfinding.MovePartialPath(navMeshAgent, this.currentTarget.Position, Time.deltaTime * speed * 100);
        }

        
        private void OnDrawGizmos()
        {
            if (this.currentTarget == null)
                return;

            Gizmos.DrawLine(this.transform.position, this.currentTarget.Position);
        }

    }
}