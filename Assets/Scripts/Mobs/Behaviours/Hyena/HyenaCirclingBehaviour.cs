using NUnit.Framework.Interfaces;
using SIGGD.Goap;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;
using Utility;
namespace SIGGD.Mobs.Hyena
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
        private AgentHuntBehaviour AgentHuntBehaviour;
        public bool finishedWalking;
        public bool finishedCircling;
        private Vector3 lastTangent = Vector3.zero;

        Animator animator;
        private void Awake()
        {
            finishedCircling = false;
            finishedWalking = false;
            rb = GetComponent<Rigidbody>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            AgentMoveBehaviour = GetComponent<AgentMoveBehaviour>();
            AgentHuntBehaviour = GetComponent<AgentHuntBehaviour>();
        }
        void Start()
        {
            beginningAttackCooldown = 0f;
        }

        public IEnumerator Circle(Func<Vector3> GetTarget)
        {
            finishedCircling = false;
            float duration = UnityEngine.Random.Range(12f, 15f);
            float elapsed = 0f;
            float maxRadius = 10f;
            float circleSpeed = 10f;
            float idealRadius = maxRadius;
            float radius = maxRadius;
            float stuckTimer = 0f;
            Vector3 lastPosition = rb.position;
            float direction = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float inwardsFactor = 0f;
            float radiusMargin = 3f;
            Vector3 lastDir = transform.forward;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float distance = Vector3.Distance(GetTarget(), transform.position);
                Vector3 toTarget = (GetTarget() - transform.position).normalized;
                Vector3 tangent = Vector3.Cross(Vector3.up, toTarget).normalized * direction;
                tangent = Vector3.Slerp(lastTangent, tangent, Time.deltaTime * 3).normalized;
                lastTangent = tangent;
                Debug.Log($"tangent{tangent}");
                Debug.Log($"lastTangent{lastTangent}");
                if (NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh) {
                    float distanceToGo = distance - idealRadius;
                    float targetInward = Mathf.Clamp(distanceToGo / radiusMargin, -1f, 1f);
                    if (Mathf.Abs(distanceToGo) < radiusMargin * 0.3f)
                        targetInward = 0f;
                    inwardsFactor = Mathf.Lerp(inwardsFactor, Mathf.Abs(targetInward), Time.deltaTime * 5f);
                    Vector3 inward = toTarget * targetInward;
                    Vector3 desired = (tangent + inward * 0.7f).normalized;
                    Debug.Log($"desired{desired}");
                    Vector3 dir;
                    NavMeshHit hit;
                    NavMesh.Raycast(transform.position, transform.position + tangent * 3f, out hit, NavMesh.AllAreas);
                    if (hit.distance < 2f)
                    {
                        Debug.Log("raycast hit");
                        idealRadius = Mathf.Clamp(idealRadius - 2, 0, maxRadius);

                    } else
                    {
                        idealRadius = Mathf.Clamp(idealRadius + 1, 0, maxRadius);
                    }
                    Vector3 nextPos = transform.position + desired * circleSpeed * Time.deltaTime;

                    if (NavMesh.SamplePosition(nextPos, out NavMeshHit hit2, 1.0f, NavMesh.AllAreas))
                        nextPos = hit2.position;

                    dir = (nextPos - transform.position).normalized;
                    Debug.Log($"inwardsFactor{inwardsFactor}");
                    if (dir == Vector3.zero)
                    {
                        dir = lastDir;
                    } else
                    {
                        lastDir = dir;
                    }
                    if (dir.sqrMagnitude > 0.001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 20 * Time.deltaTime));
                    }
                    rb.MovePosition(rb.position + dir * circleSpeed * Time.deltaTime);
                    Debug.Log($"idealRadius{idealRadius}");
                }
                if (Vector3.Distance(rb.position, lastPosition) < circleSpeed * Time.deltaTime * 0.25f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer > 1f)
                    {
                        idealRadius = Mathf.Clamp(idealRadius - 2, 0, maxRadius);
                    }
                }
                else
                {
                    stuckTimer = 0f;
                }
                lastPosition = rb.position;

                yield return new WaitForFixedUpdate(); 
            }
            finishedCircling = true;
        }
        public IEnumerator WalkTowardsTarget(Func<Vector3> GetTarget)
        {

            finishedWalking = false;
            float duration = UnityEngine.Random.Range(1f, 2f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (Vector3.Distance(GetTarget(), transform.position) < 5f)
                    break;
                Vector3 dir = (Pathfinding.MovePartialPath2(NavMeshAgent, GetTarget(), Time.deltaTime * 10) - transform.position).normalized;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 10f * Time.deltaTime));
                }
                rb.MovePosition(rb.position + dir * 10 * Time.deltaTime);
                yield return new WaitForFixedUpdate();
            }
            NavMeshAgent.isStopped = true;
            yield return new WaitUntil(() => NavMeshAgent.pathPending != true);
            finishedWalking = true;
        }
    }
}
