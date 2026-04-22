using FMOD;
using FMOD.Studio;
using SIGGD.Mobs.StateMachine;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utility;

namespace SIGGD.Mobs
{

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(StaminaBehaviour))]
    [RequireComponent(typeof(AgentData))]
    public class Movement : MonoBehaviour
    {
        [ShowInInspector] private float speed;
        [SerializeField] private float baseSpeed = 12f;
        [SerializeField] private float rotationSpeed = 25f;
        [SerializeField] private float sprintMultiplier = 1.5f;

        private MobContext ctx;
        private EventInstance footsteps;

        private StaminaBehaviour sprint;
        public bool sprintAllowed;

        private NavMeshAgent agent;
        private Rigidbody rb;
        private AgentData agentData;

        private NavMeshQueryFilter navFilter;

        private Vector3 smoothDir = Vector3.forward;     
        private Vector3 velocity = Vector3.zero;      
        private Vector3 facing = Vector3.forward;       

        private const float MinDirSqr = 0.0001f;
        private const float MinMoveSqr = 0.03f * 0.03f; 

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
            sprint = GetComponent<StaminaBehaviour>();
            agentData = GetComponent<AgentData>();

            if (agent != null)
                agent.updateRotation = false;

            speed = baseSpeed;
            facing = transform.forward;
            facing.y = 0f;
            if (facing.sqrMagnitude < 0.001f) facing = Vector3.forward;

            StartCoroutine(FindCorrectFootstepSound());
        }
        private void Start()
        {
            navFilter = agentData.filter;
        }

        private void FixedUpdate()
        {
            HandleFootstepSound();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="desiredDir"></param>
        /// <param name="speedMulti"></param>
        /// <param name="turnResponsiveness"></param>
        public void MoveTowards(Vector3 desiredDir, float speedMulti, float turnResponsiveness = 1f, bool moveWithSprint = true)
        {
            // Applies sprinting
            if (moveWithSprint && sprintAllowed && sprint != null && sprint.stamina > 0f)
            {
                speed = baseSpeed * sprintMultiplier;
                sprint.ReduceStamina(50f * Time.fixedDeltaTime);
            } else
            {
                speed = baseSpeed;
            }
            // Fall back to smoothDir
            desiredDir.y = 0f;
            if (desiredDir.sqrMagnitude < MinDirSqr)
            {
                desiredDir = smoothDir;
                if (desiredDir.sqrMagnitude < MinDirSqr)
                    return;
            }

            desiredDir.Normalize();

            // Damps direction and velocity based off of turn responsiveness
            float t = Mathf.Clamp01((turnResponsiveness - 1f) / 4f);
            float dirDamp = Mathf.Lerp(14f, 45f, t);
            float velDamp = Mathf.Lerp(10f, 28f, t);

            smoothDir = UnityUtil.DampVector3Spherical(smoothDir, desiredDir, dirDamp, Time.fixedDeltaTime);

            Vector3 targetVel = smoothDir * (speed * speedMulti);
            velocity = UnityUtil.DampVector3(velocity, targetVel, velDamp, Time.fixedDeltaTime);

            velocity.y = 0f;

            Vector3 intended = rb.position + velocity * Time.fixedDeltaTime;

            if (NavMesh.SamplePosition(intended, out NavMeshHit hit, 0.6f, navFilter))
            {
                intended = hit.position;
            }
            else
            {
                intended = rb.position + smoothDir * 0.05f;
                velocity *= 0.5f;
            }

            Vector3 delta = intended - rb.position;
            delta.y = 0f;

            rb.MovePosition(intended);

            // If movement is significant then turn in that direction
            if (delta.sqrMagnitude > MinMoveSqr)
            {
                facing = smoothDir;
                facing.y = 0f;

                if (facing.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(facing, Vector3.up);
                    rb.MoveRotation(UnityUtil.DampQuaternion(rb.rotation, targetRot, rotationSpeed, Time.fixedDeltaTime));
                }
            }

            if (agent != null)
                agent.nextPosition = intended;
        }
        public void MoveTowardsNoRotation(Vector3 desiredDir, float speedMulti, float turnResponsiveness = 1f, bool moveWithSprint = true)
        {
            // Applies sprinting
            if (moveWithSprint && sprintAllowed && sprint != null && sprint.stamina > 0f)
            {
                speed = baseSpeed * sprintMultiplier;
                sprint.ReduceStamina(50f * Time.fixedDeltaTime);
            }
            else
            {
                speed = baseSpeed;
            }
            // Fall back to smoothDir
            desiredDir.y = 0f;
            if (desiredDir.sqrMagnitude < MinDirSqr)
            {
                desiredDir = smoothDir;
                if (desiredDir.sqrMagnitude < MinDirSqr)
                    return;
            }

            desiredDir.Normalize();

            // Damps direction and velocity based off of turn responsiveness
            float t = Mathf.Clamp01((turnResponsiveness - 1f) / 4f);
            float dirDamp = Mathf.Lerp(14f, 45f, t);
            float velDamp = Mathf.Lerp(10f, 28f, t);

            smoothDir = UnityUtil.DampVector3Spherical(smoothDir, desiredDir, dirDamp, Time.fixedDeltaTime);

            Vector3 targetVel = smoothDir * (speed * speedMulti);
            velocity = UnityUtil.DampVector3(velocity, targetVel, velDamp, Time.fixedDeltaTime);

            velocity.y = 0f;

            Vector3 intended = rb.position + velocity * Time.fixedDeltaTime;

            if (NavMesh.SamplePosition(intended, out NavMeshHit hit, 0.6f, navFilter))
            {
                intended = hit.position;
            }
            else
            {
                intended = rb.position + smoothDir * 0.05f;
                velocity *= 0.5f;
            }

            Vector3 delta = intended - rb.position;

            rb.MovePosition(intended);
            if (agent != null)
                agent.nextPosition = intended;
        }

        public void EnableSprint() => sprintAllowed = true;
        public void DisableSprint() => sprintAllowed = false;

        public void InitializeMobContext(MobContext ctx)
        {
            this.ctx = ctx;
        }

        private IEnumerator FindCorrectFootstepSound()
        {
            yield return new WaitUntil(() => ctx != null);

            switch (ctx.Type)
            {
                case MobType.Hyena:
                    FMODEvents.Instance.GetEventInstance("HyenaFootstep", instance => { footsteps = instance; });
                    break;
                case MobType.Apex:
                    FMODEvents.Instance.GetEventInstance("ApexFootstep", instance => { footsteps = instance; });
                    break;
                default:
                    yield return null;
                    break;
            }
        }

        private void HandleFootstepSound()
        {
            if (!footsteps.isValid()) return;

            if ((rb.linearVelocity.magnitude >= 1))
            {
                ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(rb.position, rb.linearVelocity, transform.forward, Vector3.up);
                footsteps.set3DAttributes(attr);

                PLAYBACK_STATE playbackState;
                footsteps.getPlaybackState(out playbackState);

                if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    footsteps.start();
                }
            }
            else
            {
                footsteps.stop(STOP_MODE.IMMEDIATE);
            }
        }
    }
}
