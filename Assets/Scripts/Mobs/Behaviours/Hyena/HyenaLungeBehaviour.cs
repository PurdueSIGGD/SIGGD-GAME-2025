using Autodesk.Fbx;
using SIGGD.Goap;
using System;
using System.Collections;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.AI;
using Utility;

namespace SIGGD.Mobs.Hyena
{
    public class HyenaLungeBehaviour : MonoBehaviour
    {
        private Rigidbody rb;
        private NavMeshAgent agent;
        private AgentMoveBehaviour agentMove;
        private Movement move;
        private AgentData agentData;

        private NavMeshQueryFilter navFilter;

        public bool finishedLunging { get; private set; }
        public bool finishedExiting { get; private set; }
        public bool lungeArriving { get; private set; }
        public bool exit { get; set; }

        public float beginningAttackCooldown;
        public float arcHeight = 2f;
        public float maxLungeDistance = 20f;
        public float maxLaunchSpeed = 22f;
        public float minFlightTime = 0.30f;

        private Vector3 lungeExitDir = Vector3.forward;
        private void Awake()
        {
            move = GetComponent<Movement>();
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
            agentMove = GetComponent<AgentMoveBehaviour>();
            agentData = GetComponent<AgentData>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Start()
        {
            navFilter = agentData.filter;
            beginningAttackCooldown = 0f;
        }

        public IEnumerator Lunge(Func<Vector3> GetTarget)
        {
            exit = false;
            finishedLunging = false;
            finishedExiting = false;
            lungeArriving = false;

            if (agentMove != null) agentMove.enabled = false;


            beginningAttackCooldown = UnityEngine.Random.Range(0.03f, 0.05f);

            Vector3 target = GetTarget();
            if (target == Vector3.zero)
            {
                ExitBehaviour();
                yield break;
            }

            // Calculates the distance from the target and determines if it should continue

            Vector3 from = rb.position;
            Vector3 delta = target - from;
            float dist = new Vector3(delta.x, 0f, delta.z).magnitude;
            if (dist < 5f || dist > maxLungeDistance)
            {
                ExitBehaviour();
                yield break;
            }
            // Attempts to shift the target to the navmesh
            var shiftPos = Pathfinding.ShiftTargetToNavMesh(rb.position, navFilter, 2.5f);
            if (shiftPos != Pathfinding.ERR_VECTOR)
            {
                rb.position = shiftPos;
                transform.position = shiftPos;
            }
            
            // Stops the navmesh agent
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            Vector3 toTarget = target - rb.position;
            toTarget.y = 0f;
            Vector3 dirToTarget = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : transform.forward;


            // Aim slightly before the target
            Vector3 aimPoint = target - dirToTarget;

            if (NavMesh.SamplePosition(aimPoint, out NavMeshHit aimHit, 3f, navFilter))
                aimPoint = aimHit.position;
            Vector3 lastGoodPos = rb.position;

            // Start rotating towards the target

            float t = 0f;

            while (t < 0.85f && !exit)
            {
                t += Time.fixedDeltaTime;

                Vector3 to = aimPoint - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude < 0.0001f)
                    break;

                // If within 15 degrees then stop
                Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
                float angle = Quaternion.Angle(rb.rotation, targetRot);
                if (angle <= 15f)
                    break;

                Quaternion newRot = UnityUtil.DampQuaternion(rb.rotation, targetRot, 3f, Time.fixedDeltaTime);
                rb.MoveRotation(newRot);

                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForSeconds(beginningAttackCooldown);

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.angularVelocity = Vector3.zero;

            // Lerps arc height based off distance of lunge

            float lungeDistance = Mathf.InverseLerp(0f, maxLungeDistance, dist);
            float arc = Mathf.Lerp(0.6f, 1.4f, lungeDistance); 


            // Computes the velocity of the lunge
            if (!TryComputeVelocity(rb.position, aimPoint, arc, out Vector3 launchVel))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                if (agent != null) agent.isStopped = false;
                ExitBehaviour();
                yield break;
            }

            // Caps out the speed of the lunge
            if (launchVel.magnitude > maxLaunchSpeed)
                launchVel = launchVel.normalized * maxLaunchSpeed;

            // Saves exit dir for exiting later
            Vector3 exitDir = new Vector3(launchVel.x, 0f, launchVel.z);
            if (exitDir.sqrMagnitude > 0.001f)
                lungeExitDir = exitDir.normalized;

            rb.linearVelocity = launchVel;

            // Calculates flight time and arrive time
            float flightTime = Mathf.Max(minFlightTime, Mathf.Abs(launchVel.y / Physics.gravity.y) * 2f);
            if (flightTime < minFlightTime) flightTime = minFlightTime;

            float arriveTime = Mathf.Max(0f, flightTime - 0.15f);
            yield return new WaitForSeconds(arriveTime);
            lungeArriving = true;

            float landTimeout = Mathf.Clamp(flightTime + 0.2f, 0.35f, 0.75f);
            float landElapsed = 0f;
            int groundedFrames = 0;

            while (landElapsed < landTimeout && !exit)
            {
                landElapsed += Time.fixedDeltaTime;

                bool grounded = (agentMove != null) && agentMove.IsGrounded;
                bool notFalling = rb.linearVelocity.y <= 0.5f;

                if (grounded && notFalling)
                    groundedFrames++;
                else
                    groundedFrames = 0;

                if (groundedFrames >= 3)
                    break;

                yield return new WaitForFixedUpdate();
            }

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            shiftPos = Pathfinding.ShiftTargetToNavMesh(rb.position, navFilter, 8f);
            if (shiftPos != Pathfinding.ERR_VECTOR)
            {
                rb.position = shiftPos;
                transform.position = shiftPos;
            }
            NavMeshPath path = new NavMeshPath();

            // Warp back to starting position if new position is not connected to it
            bool hasPath = NavMesh.CalculatePath(rb.position, lastGoodPos, navFilter, path);

            if (!hasPath || path.status != NavMeshPathStatus.PathComplete)
            {
                rb.position = lastGoodPos;
                transform.position = lastGoodPos;
            }
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            if (agent != null)
            {
                agent.Warp(rb.position);
                agent.nextPosition = rb.position;
                agent.isStopped = false;
            }
            if (agentMove != null) agentMove.enabled = true;

            finishedLunging = true;
        }

        public IEnumerator ExitLunge(Func<Vector3> GetTarget)
        {
            finishedExiting = false;
            if (agentMove != null) agentMove.enabled = false;

            float duration = UnityEngine.Random.Range(1f, 2f);
            float elapsed = 0f;

            Vector3 perpendicular = Vector3.Cross(Vector3.up, lungeExitDir).normalized;
            Vector3 awayDir = (-lungeExitDir * 0.85f + perpendicular * 0.15f).normalized;

            while (elapsed < duration && !exit)
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
                if (NavMesh.SamplePosition(awayPoint, out NavMeshHit hit, 3f, navFilter))
                    awayPoint = hit.position;

                Vector3 dir = NavSteering.GetSteeringDirection(agent, awayPoint, rb.position, 0.1f);
                move.MoveTowards(dir, 1.2f);

                yield return new WaitForFixedUpdate();
            }

            if (agentMove != null) agentMove.enabled = true;
            finishedExiting = true;
        }

        public void ExitBehaviour()
        {
            lungeArriving = false;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.isKinematic = true;
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }

            if (agent != null)
            {
                agent.enabled = true;
                agent.isStopped = false;
                agent.nextPosition = rb != null ? rb.position : transform.position;
            }

            if (agentMove != null) agentMove.enabled = true;

            finishedLunging = false;
            finishedExiting = false;
            exit = true;
        }
        private static bool TryComputeVelocity(Vector3 from, Vector3 to, float arcHeight, out Vector3 velocity)
        {
            
            velocity = Vector3.zero;

            // Finds the XZ velocity and distance
            Vector3 delta = to - from;
            Vector3 deltaXZ = new Vector3(delta.x, 0f, delta.z);
            float distXZ = deltaXZ.magnitude;

            if (distXZ < 0.001f)
                return false;

            // Finds the max arc height by combining extra arc height with the minimum
            float gravity = Mathf.Abs(Physics.gravity.y);
            float arc = Mathf.Max(from.y, to.y) + Mathf.Max(0.1f, arcHeight);

            float up = arc - from.y;
            float down = arc - to.y;

            float vY = Mathf.Sqrt(2f * gravity * up);
            float tUp = vY / gravity;
            float tDown = Mathf.Sqrt(2f * down / gravity);
            float tTotal = tUp + tDown;

            if (tTotal <= 0.01f)
                return false;

            Vector3 vXZ = deltaXZ / tTotal;
            velocity = vXZ + Vector3.up * vY;
            return true;
        }
    }
}
