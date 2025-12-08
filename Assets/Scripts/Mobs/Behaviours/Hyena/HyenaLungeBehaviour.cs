using SIGGD.Goap;
using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal.Internal;
using Utility;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace SIGGD.Mobs.Hyena
{
    public class HyenaLungeBehaviour : MonoBehaviour
    {
        private Rigidbody rb;
        private NavMeshAgent NavMeshAgent;
        private AgentMoveBehaviour AgentMoveBehaviour;
        public bool finishedLunging { get; private set; } = false;
        public bool finishedExiting { get; private set; } = false;
        public bool lungeArriving { get; private set; } = false;
        public bool exit { get; set; } = false;

        private float gravity = Mathf.Abs(Physics.gravity.y);
        public float attackInterval;
        public float telegraph;
        private Movement move;
        public float beginningAttackCooldown;
        public float attackOffset;
        public float arcHeight = 2f;
        private float speed;

        private Vector3 lungeDir;

        Animator animator;
        private void Awake()
        {
            move = GetComponent<Movement>();
            rb = GetComponent<Rigidbody>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            AgentMoveBehaviour = GetComponent<AgentMoveBehaviour>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            lungeDir = Vector3.zero;
            speed = 10f;
        }
        void Start()
        {
            beginningAttackCooldown = 0f;
        }

        public IEnumerator Lunge(Func<Vector3> GetTarget)
        {
            beginningAttackCooldown = UnityEngine.Random.Range(0.03f, 0.05f);
            finishedExiting = false;
            finishedLunging = false;
            lungeArriving = false;
            Vector3 target = GetTarget();
            float distance = Vector3.Distance(target, rb.position);
            if (target == Vector3.zero || (target - transform.position).sqrMagnitude < 0.1f || distance > 20)
            {
                ExitBehaviour();
                yield break;
            }
            NavMeshAgent.enabled = false;
            Quaternion targetRot = Quaternion.LookRotation(GetTarget() - transform.position);
            float timeout = 0.5f;
            while (Mathf.Abs(Quaternion.Angle(transform.rotation, targetRot)) > 20f && timeout > 0)
            {
                timeout -= Time.fixedDeltaTime;

                transform.rotation = UnityUtil.DampQuaternion(transform.rotation,
                    targetRot, 3f, Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(beginningAttackCooldown);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = false;
            Vector3 vectDist3D = target - transform.position;
            float xDist = vectDist3D.x;
            float yDist = vectDist3D.y;
            float zDist = vectDist3D.z;
            float dist2D = Mathf.Sqrt(xDist * xDist + zDist * zDist);
            float xVelocity = (xDist * speed) / dist2D;
            float zVelocity = (zDist * speed) / dist2D;
            float yVelocity = (yDist * speed) / dist2D + (4.9f * dist2D) / speed;
            Vector3 forceVector = Vector3.ClampMagnitude(new Vector3(xVelocity, yVelocity, zVelocity), 30);
            lungeDir = UnityUtil.ToVector2(forceVector).normalized;
            rb.linearVelocity = forceVector;
            float time = (2 * yVelocity) / gravity;
            if (time < 0.25f) time = 0.25f;
            yield return new WaitForSeconds(time - 0.125f);
            lungeArriving = true;
            float elapsed = 0f;
            timeout = 0.25f;
            while (elapsed < timeout && AgentMoveBehaviour.IsGrounded)
            {
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            yield return new WaitForFixedUpdate();
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 8f, NavMesh.AllAreas))
            {
                NavMeshAgent.Warp(hit.position);
            }
            rb.isKinematic = true;
            NavMeshAgent.enabled = true;
            finishedLunging = true;
        }
        public IEnumerator ExitLunge(Func<Vector3> GetTarget)
        {
            finishedExiting = false;
            AgentMoveBehaviour.enabled = false;

            float duration = UnityEngine.Random.Range(1f, 2f);
            float elapsed = 0f;

            Vector3 awayDirPerpendicular = Vector3.Cross(lungeDir, Vector3.up).normalized;

            Vector3 awayDir = (lungeDir * 0.8f + awayDirPerpendicular * 0.2f).normalized;
  

            while (elapsed < duration)
            {
                elapsed += Time.fixedDeltaTime;
                Vector3 targetPos = GetTarget();
                if (targetPos == Vector3.zero)
                {
                    ExitBehaviour();
                    yield break;
                }
                if (Vector3.Distance(targetPos, transform.position) > 8f)
                    break;
                Vector3 awayPoint = transform.position + awayDir * 6f;
                Vector3 dir = NavSteering.GetSteeringDirection(NavMeshAgent, awayPoint, 0.1f);

                move.MoveTowards(dir, 1.2f);
                yield return new WaitForFixedUpdate();
            }

            AgentMoveBehaviour.enabled = true;
            finishedExiting = true;
        }
        public void ExitBehaviour()
        {
            lungeArriving = false;
            rb.isKinematic = true;
            NavMeshAgent.enabled = true;
            AgentMoveBehaviour.enabled = true;
            finishedLunging = false;
            finishedExiting = false;
            exit = true;
        }
    }
}