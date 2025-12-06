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
        private StaminaBehaviour sprint;
        private ITarget currentTarget;
        private bool shouldMove;
        private bool sprintAllowed;
        private Rigidbody rb;
        private Movement move;


        public NavMeshAgent navMeshAgent;


        public float speed;

        [SerializeField] public Transform groundCheckPoint;
        [SerializeField] public Vector3 groundCheckSize = new Vector3(0.49f, 0.3f, 0.49f);
        Vector3 smoothDir = Vector3.zero;
        Vector3 velocity = Vector3.zero;
        public LayerMask groundLayer;
        float nextPathUpdate = 0f;
        float pathUpdateInterval = 0.1f;
        private Vector3 cachedNavPoint = Vector3.zero;
        private Vector3 smoothedNavPoint = Vector3.zero;

        public bool IsGrounded =>
            Physics.CheckBox(groundCheckPoint.position, groundCheckSize, Quaternion.identity, groundLayer);

        private void Awake()
        {
            move = GetComponent<Movement>();
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
        

        public void FixedUpdate()
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
                    speed = 1.5f;
                    sprint.ReduceStamina(8 * Time.fixedDeltaTime);
                }
            }
            Vector3 desiredDirection = NavSteering.GetSteeringDirection(navMeshAgent, currentTarget.Position, 0.1f);

            move.MoveTowards(desiredDirection, 1.5f);

        }
        public void Move(Vector3 dir, float speed)
        {

            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(velocity, Vector3.up);
                rb.MoveRotation(UnityUtil.DampQuaternion(rb.rotation, targetRot, 12f, Time.fixedDeltaTime));
            }
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