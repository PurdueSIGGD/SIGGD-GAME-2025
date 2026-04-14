using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using SIGGD.Mobs.PackScripts;
using Utility;

namespace SIGGD.Mobs.Hyena
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(AgentData))]
    [RequireComponent(typeof(EnemyAnimator))]
    public class HyenaCirclingBehaviour : MonoBehaviour
    {
        [Tooltip("Speed multiplier of Hyena when circling around target")]
        [SerializeField] float circlingSpeedMultiplier;
        [Tooltip("Speed multiplier of Hyena when pacing before lunge")]
        [SerializeField] float lungePreparationMultiplier;

        private Rigidbody rb;
        private NavMeshAgent agent;
        private AgentMoveBehaviour agentMove;
        private PackBehavior packBehavior;
        private Movement move;
        private AgentData agentData;
        private EnemyAnimator enemyAnimator;

        private NavMeshQueryFilter navFilter;

        public bool finishedWalking;
        public bool finishedCircling;
        public bool exit;
        public bool failedWalking;
        public bool finished;

        private float offset;
        private bool hasOffset;

        [SerializeField] private const float BaseRadius = 10f;
        private const float GoalSampleDist = 3.0f;

        [SerializeField] private float AngularSpeedMin = 0.75f;
        [SerializeField] private const float AngularSpeedMax = 1.15f;

        private const float EdgeBiasRange = 3.0f;
        private const float EdgeBiasStrength = 1.0f;

        private const float MinMoveSqr = 0.03f * 0.03f;
        private const float StuckTimeToEscape = 0.55f;
        private const float EscapeRadius = 6.0f;
        private const float EscapeDuration = 0.60f;

        private float angleRad;
        private float angSpeed;
        private float radius;
        private float initialRadius;

        private bool escaping;
        private Vector3 escapeGoal;
        private float escapeTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();
            agentMove = GetComponent<AgentMoveBehaviour>();
            packBehavior = GetComponent<PackBehavior>();
            move = GetComponent<Movement>();
            agentData = GetComponent<AgentData>();
            enemyAnimator = GetComponent<EnemyAnimator>();

            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Start()
        {
            navFilter = agentData.filter;
        }
        /// <summary>
        /// Executes a behavior where the object either moves toward a target or circles around it, depending on the
        /// distance and a random factor.
        /// </summary>
        /// <remarks>The method alternates between circling the target and walking toward it based on the
        /// distance to the target and a random probability. The behavior continues until the object successfully
        /// completes its movement or an exit condition is triggered. <para> If the distance to the target is less than
        /// 9 units and a random value exceeds 0.3, the object will attempt to walk directly toward the target.
        /// Otherwise, it will alternate between circling the target and walking toward it, with a maximum of three
        /// circling attempts before exiting. </para> <para> The method respects an external exit condition, which can
        /// terminate the behavior prematurely. </para></remarks>
        /// <param name="GetTarget">A delegate that provides the current position of the target as a <see cref="Vector3"/>.</param>
        /// <returns>An enumerator that can be used to control the execution of the behavior over multiple frames.</returns>
        public IEnumerator CircleLoop(Transform target)
        {
            finished = false;
            exit = false;
            
            if (Vector3.Distance(target.position, transform.position) < 9f && UnityEngine.Random.value > 0.3f)
            {
                yield return StartCoroutine(WalkTowardsTarget(target));
                yield return new WaitUntil(() => finishedWalking || exit);
                if (exit) yield break;
            }

            int loopCount = 0;
            do
            {
                yield return StartCoroutine(Circle(target));
                yield return new WaitUntil(() => finishedCircling || exit);
                if (exit) yield break;

                yield return StartCoroutine(WalkTowardsTarget(target));
                yield return new WaitUntil(() => finishedWalking || exit);

                if (loopCount > 3) ExitBehaviour();
                if (exit) yield break;

                loopCount++;
            } while (failedWalking);

            yield return StartCoroutine(TurnTowardsTarget(target));

            finished = true;
        }

        private IEnumerator TurnTowardsTarget(Transform target)
        {
            float t = 0f;
            while (t < 2.5f && !exit)
            {
                t += Time.fixedDeltaTime;

                Vector3 targetRaw = target.position;
                if (targetRaw == Vector3.zero)
                    break;

                Vector3 to = targetRaw - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude < 0.0001f)
                    break;

                Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
                float angle = Quaternion.Angle(rb.rotation, targetRot);
                if (angle <= 10f)
                    break;

                Quaternion newRot = UnityUtil.DampQuaternion(rb.rotation, targetRot, 10f, Time.fixedDeltaTime);
                rb.MoveRotation(newRot);

                yield return new WaitForFixedUpdate();
            }
        }

        private IEnumerator Circle(Transform target)
        {
            finishedCircling = false;
            escaping = false;

            if (agentMove != null) agentMove.enabled = false;

            hasOffset = false;
            offset = 0f;

            // Apply pack offsets so that hyenas do not overlap each other when circling
            if (packBehavior != null)
            {
                hasOffset = true;
                offset = packBehavior.addRandomPackOffset(0.1f);
            } else
            {
                hasOffset = false;
                offset = 0f;
            }
            if (Mathf.Approximately(offset, 0f)) offset = UnityEngine.Random.Range(0f, 10f);


            // Pick a random angle, speed, direction, and duration
            radius = BaseRadius + offset;
            initialRadius = radius;

            
            angleRad = Random.Range(0f, Mathf.PI * 2f);
            float sign = Random.value > 0.5f ? 1f : -1f;
            angSpeed = sign * Random.Range(AngularSpeedMin, AngularSpeedMax);

            float duration = Random.Range(6f, 10f);
            float timeAtBeginning = Time.time;
            float elapsed = 0f;

            float stuckTimer = 0f;
            float updateSpeedTimer = 0f;
            float distToTargetMulti = 1f;
            float speedUpdateRate = 0.5f;
            bool nearEndOfLunge = false;
            Vector3 lastPos = rb.position;
            try
            {
                while (elapsed < duration && !exit)
                {
                    elapsed += Time.fixedDeltaTime;
                    Vector3 targetRaw = target.position;
                    if (targetRaw == Vector3.zero)
                    {
                        ExitBehaviour();
                        yield break;
                    }

                    Vector3 targetPos = SampleToNavMesh(targetRaw, 6f);

                    // Escape loop runs if hyena is stuck
                    if (escaping)
                    {
                        escapeTimer -= Time.fixedDeltaTime;

                        Vector3 e = escapeGoal - rb.position;
                        e.y = 0f;

                        if (escapeTimer <= 0f || e.sqrMagnitude < 1.0f)
                        {
                            escaping = false;
                        }
                        else
                        {
                            Vector3 exitDir = e.sqrMagnitude > 0.0001f ? e.normalized : transform.forward;
                            exitDir = ApplyEdgeAvoidance(rb.position, exitDir);
                            move.MoveTowards(exitDir, 1.2f, 6f);
                        }

                        lastPos = rb.position;
                        yield return new WaitForFixedUpdate();
                        continue;
                    }

                    // Computes a new point on the circle and samples it to the navmesh

                    angleRad += angSpeed * Time.fixedDeltaTime;

                    Vector3 ringGoalRaw = targetPos + new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * radius;

                    Vector3 ringGoal = SampleToNavMesh(ringGoalRaw, GoalSampleDist);

                    // Calculates the necessary direction and applies edge avoidance
                    Vector3 d = ringGoal - rb.position;
                    d.y = 0f;

                    Vector3 dir = d.sqrMagnitude > 0.0001f ? d.normalized : transform.forward;
                    if (nearEndOfLunge)
                    {
                        Vector3 to = targetPos - transform.position;
                        dir = UnityUtil.DampVector3Spherical(dir, to.normalized, 12f, Time.fixedDeltaTime * 3f);
                        radius = UnityUtil.Damp(radius, initialRadius / 4f, 12f, Time.fixedDeltaTime * 3f);
                    }
                    dir = ApplyEdgeAvoidance(rb.position, dir);

                    // Speed multi ranges from 1 to 3 based off of distance
                    updateSpeedTimer += Time.fixedDeltaTime;
                    if (updateSpeedTimer > speedUpdateRate)
                    {
                        distToTargetMulti = 1f + (2f * UnityUtil.ExponentialCurve(Vector3.Distance(targetPos, rb.position) / BaseRadius * 2f));
                        if (nearEndOfLunge) distToTargetMulti *= 0.5f;
                        updateSpeedTimer = 0f;
                    }


                    move.MoveTowardsNoRotation(dir, circlingSpeedMultiplier, distToTargetMulti);

                    // Face the movement direction
                    Vector3 moveDir = dir;
                    moveDir.y = 0f;
                    if (moveDir.sqrMagnitude > 0.001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
                        rb.MoveRotation(UnityUtil.DampQuaternion(rb.rotation, targetRot, 5f, Time.fixedDeltaTime));
                    }

                    // If movement is negligible for a period of time then it starts an escape sequence

                    bool notMoving = (rb.position - lastPos).sqrMagnitude < MinMoveSqr;
                    if (notMoving)
                    {
                        stuckTimer += Time.fixedDeltaTime;
                        if (stuckTimer > StuckTimeToEscape)
                        {
                            StartRandomEscape();
                            stuckTimer = 0f;
                        }
                    }
                    else
                    {
                        stuckTimer = 0f;
                    }
                    if ((Time.time - timeAtBeginning) > duration - 0.5f)
                    {
                        nearEndOfLunge = true;
                    }
                    lastPos = rb.position;
                    yield return new WaitForFixedUpdate();
                }
            }
            finally
            {
                // Makes sure that the offset is removed and movement is enabled
                if (hasOffset && packBehavior != null)
                {
                    packBehavior.removePackOffset(offset);
                    hasOffset = false;
                }

                if (agentMove != null) agentMove.enabled = true;
                finishedCircling = true;
            }
        }

        private IEnumerator WalkTowardsTarget(Transform target)
        {
            finishedWalking = false;
            failedWalking = false;

            float maxDuration = UnityEngine.Random.Range(5f, 6f);
            float elapsed = 0f;

            float stuckTimer = 0f;
            Vector3 lastPos = rb.position;

            while (elapsed < maxDuration && !exit)
            {
                elapsed += Time.fixedDeltaTime;

                Vector3 targetRaw = target.position;
                if (targetRaw == Vector3.zero)
                {
                    ExitBehaviour();
                    yield break;
                }

                Vector3 targetPos = SampleToNavMesh(targetRaw, 6f);

                Vector3 toTarget = targetPos - rb.position;
                toTarget.y = 0f;

                float dist = toTarget.magnitude;

                // If hyena is within range then it has finished walking towards target

                if (dist >= 6 && dist <= 10)
                    break;

                // If its close then back away, if its far then move closer
                Vector3 dir;
                if (dist > 10)
                {
                    dir = toTarget.normalized;
                } else {
                    dir = (-toTarget).normalized;
                }
                dir = ApplyEdgeAvoidance(rb.position, dir);
                move.MoveTowardsNoRotation(dir, lungePreparationMultiplier, 1.0f, false);

                Vector3 moveDir = dir;
                moveDir.y = 0f;
                if (moveDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
                    rb.MoveRotation(UnityUtil.DampQuaternion(rb.rotation, targetRot, 5f, Time.fixedDeltaTime));
                }



                // Not moving check similar to Circle()
                bool notMoving = (rb.position - lastPos).sqrMagnitude < MinMoveSqr;
                if (notMoving)
                {
                    stuckTimer += Time.fixedDeltaTime;
                    if (stuckTimer > 0.45f)
                    {
                        StartRandomEscape();
                        stuckTimer = 0f;
                    }
                }
                else
                {
                    stuckTimer = 0f;
                }

                lastPos = rb.position;
                yield return new WaitForFixedUpdate();
            }

            if (elapsed >= maxDuration) failedWalking = true;
            finishedWalking = true;
        }

        public void ExitBehaviour()
        {
            finishedCircling = false;
            finishedWalking = false;
            failedWalking = false;
            finished = false;

            if (hasOffset && packBehavior != null)
            {
                packBehavior.removePackOffset(offset);
                hasOffset = false;
            }

            if (agentMove != null) agentMove.enabled = true;
            exit = true;
        }

        /// <summary>
        /// Applies edge avoidance by biasing the direction towards the normal of the nearest navmesh edge
        /// </summary>
        /// <remarks>It finds the closest edge of the agents navmesh.
        /// It then finds the normal of the edge hit while ignoring the y component. 
        /// The dir is then blended with the inward factor which has more influence based off of the distance of the hit and EdgeBiasStrength</remarks>
        /// <param name="pos">The position to check for the nearest edges</param>
        /// <param name="dir">The direction to apply avoidance to</param>
        /// <returns>The new direction</returns>
        private Vector3 ApplyEdgeAvoidance(Vector3 pos, Vector3 dir)
        {
            if (NavMesh.FindClosestEdge(pos, out NavMeshHit edgeHit, navFilter))
            {
                float t = Mathf.Clamp01((EdgeBiasRange - edgeHit.distance) / EdgeBiasRange);
                if (t > 0f)
                {
                    Vector3 inward = edgeHit.normal;
                    inward.y = 0f;
                    if (inward.sqrMagnitude > 0.0001f)
                    {
                        dir = (dir + inward.normalized * (EdgeBiasStrength * t)).normalized;
                    }
                }
            }
            return dir;
        }

        /// <summary>
        /// Starts a random escape sequence by finding a random point on the navmesh in a circle around the hyena
        /// </summary>
        private void StartRandomEscape()
        {
            Vector2 r = UnityEngine.Random.insideUnitCircle;
            if (r.sqrMagnitude < 0.0001f) r = Vector2.right;
            r.Normalize();

            Vector3 candidate = rb.position + new Vector3(r.x, 0f, r.y) * EscapeRadius;

            // Samples the candidate to the navmesh
            // Starts the escape timer and sets the escape goal to the position of the hit
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 4f, navFilter))
            {
                escaping = true;
                escapeGoal = hit.position;
                escapeTimer = EscapeDuration;
            }
        }

        private Vector3 SampleToNavMesh(Vector3 p, float maxDist)
        {
            if (NavMesh.SamplePosition(p, out NavMeshHit hit, maxDist, navFilter))
                return hit.position;
            return p;
        }
    }
}
