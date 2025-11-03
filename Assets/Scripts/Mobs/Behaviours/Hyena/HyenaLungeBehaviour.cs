using Autodesk.Fbx;
using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Utility;
using SIGGD.Goap;

namespace SIGGD.Mobs.Hyena
{
    public class HyenaLungeBehaviour : MonoBehaviour
    {
        public float attackInterval;
        public float telegraph;
        public float beginningAttackCooldown;
        public float attackOffset;
        private Rigidbody rb;
        private NavMeshAgent NavMeshAgent;
        private AgentMoveBehaviour AgentMoveBehaviour;
        public bool finishedLunging;
        public bool finishedExiting;
        public bool lungeArriving;
        public bool exit = false;

        Animator animator;
        private void Awake()
        {
            finishedLunging = false;
            finishedExiting = false;
            rb = GetComponent<Rigidbody>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            AgentMoveBehaviour = GetComponent<AgentMoveBehaviour>();
          //  rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        void Start()
        {
            beginningAttackCooldown = 0f;
        }

        public IEnumerator Lunge(Func<Vector3> GetTarget)
        {
            beginningAttackCooldown = UnityEngine.Random.Range(0.03f, 0.05f);

            finishedLunging = false;
            lungeArriving = false;
            Vector3 target = GetTarget();
            if ((target - transform.position).sqrMagnitude < 0.1f)
            {
                ExitBehaviour();
                yield break;
            }
            NavMeshAgent.enabled = false;
            Quaternion targetRot = Quaternion.LookRotation(GetTarget() - transform.position);
            float elapsed = 0.0f;
            float timeout = 2f;
            while (Mathf.Abs(Quaternion.Angle(transform.rotation, targetRot)) > 20f && timeout > 0)
            {
                timeout -= Time.deltaTime;
                elapsed += Time.deltaTime;

                transform.rotation = UnityUtil.DampQuaternion(transform.rotation,
                    targetRot, 0.1f, elapsed * 10);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(beginningAttackCooldown);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = false;
            float gravity = Math.Abs(Physics.gravity.y);
            float xDistance = target.x - transform.position.x;
            float zDistance = target.z - transform.position.z;
            Vector3 flatDir = new Vector3(xDistance, 0, zDistance).normalized;
            float horizontalDistance = new Vector2(xDistance, zDistance).magnitude;
            float arcHeight = 0.5f;
            float yVelocity = Mathf.Sqrt(2 * gravity * arcHeight);
            float time = (2 * yVelocity) / gravity;
            float horizontalSpeed = horizontalDistance / time;
            Vector3 forceVector = flatDir * horizontalSpeed + Vector3.up * yVelocity;
            rb.linearVelocity = forceVector;
            if (time < 0.25f) time = 0.25f;
            yield return new WaitForSeconds(time - 0.125f);
            lungeArriving = true;
            yield return new WaitForSeconds(0.125f);
            //yield return new WaitUntil(() => AgentMoveBehaviour.IsGrounded);
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            yield return new WaitForFixedUpdate();
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 8f, NavMesh.AllAreas))
            {
                NavMeshAgent.Warp(hit.position);
            }
            rb.isKinematic = true;
            yield return new WaitForFixedUpdate();
            NavMeshAgent.enabled = true;
            finishedLunging = true;
        }
        public IEnumerator ExitLunge(Func<Vector3> GetTarget)
        {
            finishedExiting = false;
            float duration = UnityEngine.Random.Range(1f, 2f);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (Vector3.Distance(GetTarget(), transform.position) > 5f)
                    break;
                Vector3 dir = (transform.position - Pathfinding.MovePartialPath2(NavMeshAgent, GetTarget(), Time.deltaTime * 10)).normalized;
                //Vector3 dir = (transform.position - GetTarget()).normalized;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 10f * Time.deltaTime));
                }
                rb.MovePosition(rb.position + dir * 10 * Time.deltaTime);
                yield return new WaitForFixedUpdate();
            }
            //NavMeshAgent.isStopped = true;
            AgentMoveBehaviour.enabled = true;
            finishedExiting = true;
        }
        public void ExitBehaviour()
        {
            lungeArriving = false;
            rb.isKinematic = true;
            NavMeshAgent.enabled = true;
            AgentMoveBehaviour.enabled = true;
            
            finishedLunging = true;
            finishedExiting = true;
            exit = true;
        }
    }
}