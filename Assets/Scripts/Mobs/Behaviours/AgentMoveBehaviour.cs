using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;
using SIGGD.Goap;
using UnityEngine.Polybrush;

namespace SIGGD.Mobs
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        
        private AgentBehaviour agent;
        private StaminaBehaviour sprint;
        private ITarget currentTarget;
        private bool shouldMove;
        private bool sprintAllowed;

        private Rigidbody rb;        


        public NavMeshAgent navMeshAgent;


        public float speed;

        [SerializeField] public Transform groundCheckPoint;
        [SerializeField] public Vector3 groundCheckSize = new Vector3(0.49f, 0.3f, 0.49f);
        public LayerMask groundLayer;

        public bool IsGrounded =>
            Physics.CheckBox(groundCheckPoint.position, groundCheckSize, Quaternion.identity, groundLayer);
        //Vector3 dest = null;

        private void Awake()
        {
            this.agent = this.GetComponent<AgentBehaviour>();
            sprint = GetComponent<StaminaBehaviour>();
            rb = GetComponent<Rigidbody>();
            sprintAllowed = false;
            speed = 5f;
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
        

        public void Update()
        {
            if (this.agent.IsPaused)
                return;

            if (!this.shouldMove)
                return;

            if (this.currentTarget == null)
                return;
            speed = 5f;
            if (sprintAllowed)
            {
                if (sprint.stamina > 0)
                {
                    speed = 11f;
                    sprint.ReduceStamina(8 * Time.deltaTime);
                }
            }

            // Move the agent along towards their goal position
            Vector3 dir = (Pathfinding.MovePartialPath2(navMeshAgent, this.currentTarget.Position, Time.deltaTime * speed) - transform.position).normalized;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, speed * Time.deltaTime));
            }
            Debug.DrawRay(agent.transform.position, dir * speed * Time.deltaTime * 100, Color.green);
            rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
            //Add Navmesh
            //Pathfinding.MovePartialPath(navMeshAgent, this.currentTarget.Position, Time.deltaTime * 100);
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
        public void EnableSprint()
        {
            sprintAllowed = true;
        }
        public void DisableSprint()
        {
            sprintAllowed = false;
        }

    }
}