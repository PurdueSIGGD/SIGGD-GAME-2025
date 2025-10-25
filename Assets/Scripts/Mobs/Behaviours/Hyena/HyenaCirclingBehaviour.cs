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
    public class HyenaCirclingBehaviour : MonoBehaviour
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

        public IEnumerator Circle(Vector3 target)
        {
            beginningAttackCooldown = UnityEngine.Random.Range(0.3f, 0.5f);
            finished = false;
            float duration = 0.2f;
            float elapsed = 0.0f;
            AgentMoveBehaviour.enabled = false;
            NavMeshAgent.enabled = false;
            rb.isKinematic = true;
            Quaternion startRot = transform.rotation;
            while (Mathf.Abs(Quaternion.Angle(transform.rotation, Quaternion.LookRotation(target - transform.position))) > 10f)
            {
                elapsed += Time.deltaTime;

                transform.position += Vector3.Cross(target - transform.position, Vector3.up);
                yield return null;
            }
            yield return new WaitForSeconds(beginningAttackCooldown);
            duration = 0.3f;
            elapsed = 0.0f;
            Vector3 startPos = transform.position;
            while (elapsed < duration)
            {
                transform.position += Vector3.Cross(target - transform.position, Vector3.up);
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, target, t) + Vector3.up * 4 * t * (1 - t);
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
            rb.isKinematic = false;
            finished = true;
        }
    }
}
