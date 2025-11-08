using SIGGD.Goap;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using SIGGD.Mobs.PackScripts;
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
        private PackBehavior PackBehavior;
        public bool finishedWalking;
        public bool finishedCircling;
        private Vector3 lastTangent = Vector3.zero;
        float offset = 0f;
        public bool exit = false;
        public bool failedWalking = false;
        public bool finished = false;

        Animator animator;
        private void Awake()
        {
            finishedCircling = false;
            finishedWalking = false;
            finished = false;
            rb = GetComponent<Rigidbody>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            AgentMoveBehaviour = GetComponent<AgentMoveBehaviour>();
            AgentHuntBehaviour = GetComponent<AgentHuntBehaviour>();
            PackBehavior = GetComponent<PackBehavior>();
        }
        void Start()
        {
            beginningAttackCooldown = 0f;
        }
        public IEnumerator CircleLoop(Func<Vector3> GetTarget)
        {
            finished = false;
            int loopCount = 0;
            do
            {
                yield return StartCoroutine(Circle(GetTarget));
                yield return new WaitUntil(() => finishedCircling || exit);
                if (exit) yield break;
                yield return StartCoroutine(WalkTowardsTarget(GetTarget));
                yield return new WaitUntil(() => finishedWalking || exit);
                if (loopCount > 3) ExitBehaviour();
                if (exit) yield break;
                loopCount++;
            } while (failedWalking);
            finished = true;
        }
        public IEnumerator Circle(Func<Vector3> GetTarget)
        {
            AgentMoveBehaviour.enabled = false;
            NavMeshAgent.enabled = false;
            offset = PackBehavior.addRandomPackOffset(0.1f);
            if (offset == 0f)
            {
                offset = UnityEngine.Random.Range(0f, 10f);
            }
            finishedCircling = false;
            float duration = UnityEngine.Random.Range(7f, 15f);
            float elapsed = 0f;
            float maxRadius = 10f + offset;
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
                if (exit) yield break;
                elapsed += Time.fixedDeltaTime;
                float distance = Vector3.Distance(GetTarget(), transform.position);
                if (distance > 50f)
                {
                    ExitBehaviour();
                    yield break;
                }
                Vector3 toTarget = (GetTarget() - transform.position).normalized;
                Vector3 tangent = Vector3.Cross(Vector3.up, toTarget).normalized * direction;
                tangent = UnityUtil.DampVector3Spherical(lastTangent, tangent, 3f, Time.fixedDeltaTime).normalized;
                lastTangent = tangent;
                float distanceToGo = distance - idealRadius;
                float targetInward = Mathf.Clamp(distanceToGo / radiusMargin, -1f, 1f);
                if (Mathf.Abs(distanceToGo) < radiusMargin * 0.3f)
                    targetInward = 0f;
                inwardsFactor = UnityUtil.Damp(inwardsFactor, Mathf.Abs(targetInward), 5f, Time.fixedDeltaTime);
                Vector3 inward = toTarget * targetInward;
                Vector3 desired = (tangent + inward * 0.7f).normalized;
                Vector3 dir;
                NavMesh.Raycast(transform.position, transform.position + tangent * 3f, out NavMeshHit hit1, NavMesh.AllAreas);
                if (hit1.distance < 3f)
                {
                    idealRadius = Mathf.Clamp(idealRadius - 2, 0, maxRadius);

                } else
                {
                    idealRadius = Mathf.Clamp(idealRadius + 0.5f, 0f, maxRadius);
                }
                Vector3 nextPos = transform.position + desired * circleSpeed * Time.fixedDeltaTime;

                if (NavMesh.SamplePosition(nextPos, out NavMeshHit hit2, 1.0f, NavMesh.AllAreas))
                    nextPos = hit2.position;

                dir = (nextPos - transform.position).normalized;
                if (dir == Vector3.zero)
                {
                    dir = lastDir;
                }
                Vector3 away = Vector3.zero;
                int count = 0;
                foreach (Collider col in Physics.OverlapSphere(transform.position, 3f, LayerMask.GetMask("Mob"))) {
                    if (col.attachedRigidbody == rb) continue;
                    away += (transform.position - col.transform.position);
                    count++;
                }
                if (count > 0)
                {
                    away /= count;
                    dir = UnityUtil.DampVector3Spherical(lastDir, (dir + away).normalized, 2f, Time.fixedDeltaTime);
                }
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                    rb.MoveRotation(UnityUtil.DampQuaternion(rb.rotation, targetRot, 20f, Time.fixedDeltaTime));
                }
                rb.MovePosition(rb.position + dir * circleSpeed * Time.fixedDeltaTime);
                lastDir = dir;
                if (Vector3.Distance(rb.position, lastPosition) < circleSpeed * Time.fixedDeltaTime * 0.25f)
                {
                    stuckTimer += Time.fixedDeltaTime;
                    if (stuckTimer > 1f)
                    {
                        idealRadius = Mathf.Clamp(idealRadius - 2f, 0, maxRadius);
                    }
                }
                else
                {
                    stuckTimer = 0f;
                }
                lastPosition = rb.position;

                yield return new WaitForFixedUpdate(); 
            }
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit3, 1f, NavMesh.AllAreas))
            {
                NavMeshAgent.Warp(hit3.position);
            }
            NavMeshAgent.enabled = true;
            PackBehavior.removePackOffset(offset);
            finishedCircling = true;
        }
        public IEnumerator WalkTowardsTarget(Func<Vector3> GetTarget)
        {
            finishedWalking = false;
            float maxDuration = UnityEngine.Random.Range(5f, 6f);
            float elapsed = 0f;
            float randDist = UnityEngine.Random.Range(3f, 10f);

            while (elapsed < maxDuration)
            {
                elapsed += Time.fixedDeltaTime;
                if (Vector3.Distance(GetTarget(), transform.position) < randDist)
                    break;
                Vector3 dir = (Pathfinding.MovePartialPath2(NavMeshAgent, GetTarget(), Time.fixedDeltaTime * 10) - transform.position).normalized;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                    rb.MoveRotation(UnityUtil.DampQuaternion(rb.rotation, targetRot, 10f, Time.fixedDeltaTime));
                }
                rb.MovePosition(rb.position + dir * 10 * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitUntil(() => NavMeshAgent.pathPending != true);
            if (elapsed >= maxDuration)
            {
                failedWalking = true;
            }
            finishedWalking = true;
        }
        public void ExitBehaviour()
        {
            finishedCircling = false;
            finishedWalking = false;
            finished = false;
            NavMeshAgent.enabled = true;
            AgentMoveBehaviour.enabled = true;
            PackBehavior.removePackOffset(offset);
            exit = true;
        }
    }
}
