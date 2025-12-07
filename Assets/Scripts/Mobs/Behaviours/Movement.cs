using UnityEngine;
using UnityEngine.AI;
using Utility;
using SIGGD.Mobs;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem.Android;
namespace SIGGD.Mobs
{
    public class Movement : MonoBehaviour
    {
        [ShowInInspector]
        private float speed;
        public float baseSpeed = 12f;
        public float rotationSpeed = 25f;
        public float pathUpdateRate = 0.2f;
        private StaminaBehaviour sprint;
        public bool sprintAllowed;

        private NavMeshAgent agent;
        private Rigidbody rb;

        private Vector3 smoothDir = Vector3.forward;
        private Vector3 velocity = Vector3.zero;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
            sprint = GetComponent<StaminaBehaviour>();
            agent.updateRotation = false;
           // agent.updatePosition = false;
            speed = baseSpeed;
        }

        public void MoveTowards(Vector3 desiredDir, float speedMulti)
        {
            
            if (sprintAllowed) {
                if (sprint.stamina > 0) {
                    speed = baseSpeed * 1.5f;
                    sprint.ReduceStamina(50 * Time.fixedDeltaTime);
                }
            } else
            {
                speed = baseSpeed;
            }
            if (desiredDir.sqrMagnitude < 0.0001f)
                return;
            smoothDir = UnityUtil.DampVector3Spherical(smoothDir, desiredDir, 14f, Time.fixedDeltaTime);

            velocity = UnityUtil.DampVector3(velocity, smoothDir * speed * speedMulti, 10f, Time.fixedDeltaTime);

            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            Quaternion targetRot = Quaternion.LookRotation(velocity, Vector3.up);
            rb.MoveRotation(UnityUtil.DampQuaternion(rb.rotation, targetRot, rotationSpeed, Time.fixedDeltaTime));
            agent.nextPosition = rb.position;
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