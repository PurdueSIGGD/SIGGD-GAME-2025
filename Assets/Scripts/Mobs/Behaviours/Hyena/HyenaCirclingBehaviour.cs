using SIGGD.Goap;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using SIGGD.Mobs.PackScripts;
using Utility;
using JetBrains.Annotations;
using Unity.Cinemachine;
using SIGGD.Goap.Config;
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
        private HyenaStats circleConfig;
        private PackBehavior PackBehavior;
        private Movement move;
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
            move = GetComponent<Movement>();

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
            if (Vector3.Distance(GetTarget(), transform.position) < 9f && UnityEngine.Random.Range(0f, 1f) > 0.3f)
            {
                yield return StartCoroutine(WalkTowardsTarget(GetTarget));
                yield return new WaitUntil(() => finishedWalking || exit);
                if (exit) yield break;
            }
            else
            {
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
            }
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
            float edgeCheckTimer = 0f;
            float edgeCheckInterval = 0.25f;

            while (elapsed < duration)
            {
                if (exit) yield break;

                elapsed += Time.fixedDeltaTime;

                Vector3 targetPos = GetTarget();
                float distance = Vector3.Distance(targetPos, rb.position);

                if (distance > 50f)
                {
                    ExitBehaviour();
                    yield break;
                }

                Vector3 toTarget = (targetPos - rb.position).normalized;

                Vector3 tangent = Vector3.Cross(Vector3.up, toTarget).normalized;

                tangent = UnityUtil.DampVector3Spherical(lastTangent, tangent, 6f, Time.fixedDeltaTime).normalized;
                lastTangent = tangent;

                float distanceToGo = distance - idealRadius;
                float targetInward = Mathf.Clamp(distanceToGo / radiusMargin, -1f, 1f);

                if (Mathf.Abs(distanceToGo) < radiusMargin * 0.3f)
                    targetInward = 0f;

                inwardsFactor = UnityUtil.Damp(inwardsFactor, Mathf.Abs(targetInward), 5f, Time.fixedDeltaTime);

                Vector3 inward = toTarget * inwardsFactor;
                Vector3 desired = (tangent + inward * 0.7f).normalized;

                edgeCheckTimer += Time.fixedDeltaTime;

                if (edgeCheckTimer >= edgeCheckInterval)
                {
                    edgeCheckTimer = 0;

                    if (NavMesh.Raycast(rb.position, rb.position + tangent * 3f, out NavMeshHit hit1, NavMesh.AllAreas))
                    {
                        idealRadius = Mathf.Lerp(idealRadius, Mathf.Max(idealRadius - 1.5f, 0f), 0.8f);
                        tangent = Quaternion.AngleAxis(15, Vector3.up) * tangent;
                    }

                    if (NavMesh.FindClosestEdge(rb.position, out NavMeshHit edge, NavMesh.AllAreas) && edge.distance < 0.3f)
                    {
                        Vector3 away = (rb.position - edge.position).normalized;
                        desired = Vector3.Lerp(desired, away, 0.3f).normalized;

                        if (edge.distance < 0.05f)
                        {
                            ExitBehaviour();
                            yield break;
                        }
                    }
                }

                Vector3 awayForce = Vector3.zero;
                int count = 0;

                foreach (Collider col in Physics.OverlapSphere(rb.position, 3f, LayerMask.GetMask("Mob")))
                {
                    if (col.attachedRigidbody == rb) continue;
                    Vector3 toSelf = rb.position - col.transform.position;
                    float d = toSelf.magnitude;
                    if (d < 0.001f) continue;

                    float weight = 1f - Mathf.Clamp01(d / 3f);
                    awayForce += toSelf.normalized * weight;
                    count++;
                }

                if (count > 0)
                {
                    awayForce /= count;
                    desired = UnityUtil.DampVector3Spherical(lastDir, (desired + awayForce).normalized, 2f, Time.fixedDeltaTime);
                }
                Vector3 nextPos = rb.position + desired * circleSpeed * Time.fixedDeltaTime;

                if (NavMesh.SamplePosition(nextPos, out var hit, 3f, NavMesh.AllAreas)) {
                    nextPos = hit.position;
                }
                Vector3 dir = (nextPos - rb.position).normalized;
                rb.MovePosition(rb.position + dir * circleSpeed * Time.fixedDeltaTime);

                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                rb.MoveRotation(UnityUtil.DampQuaternion(rb.rotation, targetRot, 10f, Time.fixedDeltaTime));

                if (Vector3.Distance(rb.position, lastPosition) < circleSpeed * Time.fixedDeltaTime * 0.25f)
                {
                    stuckTimer += Time.fixedDeltaTime;

                    if (stuckTimer > 1f)
                        idealRadius = Mathf.Clamp(idealRadius - 2f, 0, maxRadius);
                }
                else
                {
                    stuckTimer = 0f;
                }
                lastDir = desired;
                lastPosition = rb.position;

                yield return new WaitForFixedUpdate();
            }

            if (NavMesh.SamplePosition(rb.position, out NavMeshHit hit3, 1f, NavMesh.AllAreas))
                NavMeshAgent.Warp(hit3.position);

            NavMeshAgent.enabled = true;
            PackBehavior.removePackOffset(offset);
            finishedCircling = true;
        }
        public IEnumerator WalkTowardsTarget(Func<Vector3> GetTarget)
        {
            AgentMoveBehaviour.enabled = false;
            finishedWalking = false;
            failedWalking = false;

            float maxDuration = UnityEngine.Random.Range(5f, 6f);
            float elapsed = 0f;
            float stopDist = UnityEngine.Random.Range(3f, 10f);

            Vector3 currentdir = transform.forward;

            float speed = 10f;

            while (elapsed < maxDuration && !exit)
            {
                elapsed += Time.fixedDeltaTime;

                Vector3 targetPos = GetTarget();
                if (Vector3.Distance(targetPos, transform.position) < stopDist)
                    break;

                Vector3 dir = NavSteering.GetSteeringDirection(NavMeshAgent, targetPos, 0.1f);
                move.MoveTowards(dir, 1.2f);
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