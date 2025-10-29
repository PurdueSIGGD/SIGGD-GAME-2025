using Autodesk.Fbx;
using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;
using Utility;

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
        public bool finished;
        public bool lungeArriving;

        Animator animator;
        private void Awake()
        {
            finished = false;
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
            Debug.Log("lunge start");
            beginningAttackCooldown = UnityEngine.Random.Range(0.03f, 0.05f);

            finished = false;
            lungeArriving = false;
            Vector3 target = GetTarget();
            if ((target - transform.position).sqrMagnitude < 0f)
            {
                finished = true;
                yield break;
            }
            AgentMoveBehaviour.enabled = false;
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
                yield return null;
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
            if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
            {
                NavMeshAgent.Warp(hit.position);
            }
            //NavMeshAgent.Warp(transform.position);
            rb.isKinematic = true;

            NavMeshAgent.enabled = true;
            AgentMoveBehaviour.enabled = true;
            finished = true;
        }
    }
}
/*
while (elapsed < duration)
{
    elapsed += Time.deltaTime;
    float t = elapsed / duration;
    rb.MovePosition(Vector3.Lerp(startPos, target, t) + Vector3.up * 2 * t * (1 - t));
    //transform.rotation = Quaternion.Slerp(startRot,
    // Quaternion.LookRotation(target - transform.position), t);
    //transform. MovePosition(position);
    yield return null;
}
*/
/*
using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Timers;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;
using Utility;
namespace SIGGD.Goap.Behaviours
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
        public bool finished;

        Animator animator;
        private void Awake()
        {
            finished = false;
            rb = GetComponent<Rigidbody>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            AgentMoveBehaviour = GetComponent<AgentMoveBehaviour>();
        }
        void Start()
        {
            beginningAttackCooldown = 0f;
        }

        public IEnumerator Lunge(Vector3 target)
        {
            beginningAttackCooldown = UnityEngine.Random.Range(0.3f, 0.5f);
            finished = false;
            float duration = 0.2f;
            float elapsed = 0.0f;
            AgentMoveBehaviour.enabled = false;
            NavMeshAgent.enabled = false;
            //rb.isKinematic = true;
            Quaternion startRot = transform.rotation;
            while (Mathf.Abs(Quaternion.Angle(transform.rotation, Quaternion.LookRotation(target - transform.position))) > 10f) 
            {
                elapsed += Time.deltaTime;
      
                transform.rotation = UnityUtil.DampQuaternion(transform.rotation, 
                    Quaternion.LookRotation(target - transform.position), 0.1f, elapsed);
                yield return null;
            }
            yield return new WaitForSeconds(beginningAttackCooldown);
            duration = 0.3f;
            elapsed = 0.0f;
            Vector3 startPos = transform.position;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rb.MovePosition(Vector3.Lerp(startPos, target, t) + Vector3.up * 2 * t * (1-t));
                //transform.rotation = Quaternion.Slerp(startRot,
   // Quaternion.LookRotation(target - transform.position), t);
                //transform. MovePosition(position);
                yield return null;
            }
            Quaternion endRotation = transform.rotation;
            NavMeshAgent.enabled = true;
            NavMeshAgent.Warp(transform.position);
            transform.rotation = endRotation;
            AgentMoveBehaviour.enabled = true;
           // rb.isKinematic = false;
            finished = true;
        }
    }
}
*/
