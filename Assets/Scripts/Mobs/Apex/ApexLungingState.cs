using CrashKonijn.Agent.Core;
using SIGGD.Goap;
using SIGGD.Mobs.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using Utility;

public class ApexLungingState : IMobState
{
    private readonly Apex apex;

    private ApexTarget target;
    private Vector3 killPosition;
    private bool hasAttacked;
    private readonly MobContext ctx;

    private Rigidbody rb;
    private NavMeshAgent agent;

    private bool launched;
    private float timer;

    private const float MaxLungeSpeed = 22f;
    private const float ArcHeight = 2f;
    private const float MinFlightTime = 0.30f;
    private const float WindupTime = 0.15f;


    public ApexLungingState(Apex apex)
    {
        this.apex = apex;
        this.ctx = apex.Context;
    }
    public void SetTarget(ApexTarget apexTarget, Vector3 position)
    {
        target = apexTarget;
        killPosition = position;
    }

    public void Enter()
    {
        rb = ctx.Rigidbody;
        agent = ctx.NavAgent;

        launched = false;
        timer = 0f;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        hasAttacked = false;
        apex.ApexLog($"Entering LungingState — target '{(target != null ? target.gameObject.name : "null")}' at {killPosition}.");
    }

    public void FixedUpdate() { }

    public void Update() {

        if (target == null)
        {
            apex.RoamingState.SetGuardPosition(rb.position);
            apex.StateMachine.ChangeState(apex.RoamingState);
            return;
        }

        Vector3 flatDelta = killPosition - rb.position;
        flatDelta.y = 0f;
        float dist = flatDelta.magnitude;

        if (dist <= apex.AttackRange)
        {
            apex.AttackingState.SetTarget(target, killPosition);
            apex.StateMachine.ChangeState(apex.AttackingState);
            return;
        }

        if (dist > apex.LungeRange)
        {
            apex.ChasingState.SetTarget(target);
            apex.StateMachine.ChangeState(apex.ChasingState);
            return;
        }
        if (!launched)
        {
            timer += Time.deltaTime;

            if (flatDelta.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDelta.normalized, Vector3.up);
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 360f * Time.deltaTime));
            }

            if (timer < WindupTime)
                return;

            if (!TryComputeVelocity(rb.position, killPosition, ArcHeight, out Vector3 launchVel))
            {
                apex.ChasingState.SetTarget(target);
                apex.StateMachine.ChangeState(apex.ChasingState);
                return;
            }

            if (launchVel.magnitude > MaxLungeSpeed)
                launchVel = launchVel.normalized * MaxLungeSpeed;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = launchVel;

            launched = true;
            timer = Mathf.Max(MinFlightTime, Mathf.Abs(launchVel.y / Physics.gravity.y) * 2f);

            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        float finalDist = Vector3.Distance(rb.position, target.transform.position);
        if (finalDist <= apex.AttackRange)
        {
            apex.AttackingState.SetTarget(target, target.transform.position);
            apex.StateMachine.ChangeState(apex.AttackingState);
        }
        else
        {
            apex.ChasingState.SetTarget(target);
            apex.StateMachine.ChangeState(apex.ChasingState);
        }
    }
    public void Exit()
    {
        if (agent != null)
            agent.isStopped = false;
        rb.isKinematic = true;
        rb.useGravity = false;
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
