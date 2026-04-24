using SIGGD.Mobs.StateMachine;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// The Apex is within attack range of a target. It stops and triggers a one-shot
/// overlap-sphere attack via <see cref="Apex.DoAttack"/>. After the attack it
/// transitions to <see cref="ApexRoamingState"/> around the kill position.
/// </summary>
public class ApexAttackingState : IMobState
{
    private readonly Apex apex;
    private readonly MobContext ctx;

    private Rigidbody rb;
    private NavMeshAgent agent;

    private ApexTarget target;
    private Vector3 killPosition;
    private bool hasAttacked;

    private bool launched;
    private float timer;

    public ApexAttackingState(Apex apex)
    {
        this.apex = apex;
        this.ctx = apex.Context;
    }

    /// <summary>Set the attack target and roam-after-kill position before transitioning into this state.</summary>
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
        apex.ApexLog($"Entering AttackingState — target '{(target != null ? target.gameObject.name : "null")}' at {killPosition}.");
    }

    public void FixedUpdate() { }

    public void Update()
    {

        if (target == null)
        {
            apex.RoamingState.SetGuardPosition(rb.position);
            apex.StateMachine.ChangeState(apex.RoamingState);
            return;
        }

        Vector3 flatDelta = killPosition - rb.position;
        flatDelta.y = 0f;
        float dist = flatDelta.magnitude;

        if (dist > apex.AttackRange)
        {
            apex.ChasingState.SetTarget(target);
            apex.StateMachine.ChangeState(apex.ChasingState);
            return;
        }
        if (!launched)
        {
            apex.SetAttacking(true);
            timer += Time.deltaTime;

            if (flatDelta.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDelta.normalized, Vector3.up);
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 360f * Time.deltaTime));
            }

            if (timer < apex.WindupTime)
                return;

            if (!TryComputeVelocity(rb.position, killPosition, apex.ArcHeight, out Vector3 launchVel))
            {
                apex.ChasingState.SetTarget(target);
                apex.StateMachine.ChangeState(apex.ChasingState);
                return;
            }

            if (launchVel.magnitude > apex.MaxLungeSpeed)
                launchVel = launchVel.normalized * apex.MaxLungeSpeed;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = launchVel;

            launched = true;
            timer = Mathf.Max(apex.MinFlightTime, Mathf.Abs(launchVel.y / Physics.gravity.y) * 2f);

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
            Attack();
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
        apex.SetAttacking(false);
        apex.ApexLog("Exiting AttackingState.");

    }

    private void Attack()
    {
        if (!hasAttacked)
        {
            hasAttacked = true;
            apex.ApexLog("AttackingState — performing attack.");
            apex.DoAttack();
            apex.ApexLog("AttackingState — attack complete, switching to RoamingState.");
            apex.RoamingState.SetGuardPosition(killPosition);
            apex.StateMachine.ChangeState(apex.RoamingState);
        }
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