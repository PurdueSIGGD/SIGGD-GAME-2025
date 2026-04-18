using UnityEngine;

namespace SIGGD.Mobs.StateMachine
{
    /// <summary>
    /// Moves a mob to a bait position, keeps it there for the configured duration,
    /// then returns it to the default wander state.
    /// </summary>
    public class BaitedState : IMobState
    {
        private readonly MobContext ctx;
        private readonly MobStateMachine stateMachine;
        private readonly IMobState returnState;
        private readonly float moveSpeedMultiplier;
        private readonly float turnResponsiveness;
        private readonly float arrivalDistance;
        private GameObject baitObject;

        private Vector3 baitPosition;
        private float holdDuration;
        private float holdTimer;
        private bool hasBaitTarget;
        private bool hasArrived;

        /// <summary>
        /// Creates a bait reaction state that always returns to the provided fallback state.
        /// </summary>
        public BaitedState(MobContext context, MobStateMachine machine, IMobState fallbackState,
                           float moveSpeedMultiplier, float turnResponsiveness, float arrivalDistance)
        {
            ctx = context;
            stateMachine = machine;
            returnState = fallbackState;
            this.moveSpeedMultiplier = moveSpeedMultiplier;
            this.turnResponsiveness = turnResponsiveness;
            this.arrivalDistance = Mathf.Max(0.05f, arrivalDistance);
        }

        /// <summary>
        /// Updates the bait destination and the amount of time the mob should remain there.
        /// </summary>
        public void Configure(GameObject baitObject, Vector3 position, float duration)
        {
            this.baitObject = baitObject;
            baitPosition = position;
            holdDuration = Mathf.Max(0f, duration);
            hasBaitTarget = true;
            hasArrived = false;
        }

        public void Enter()
        {
            ctx.Movement.DisableSprint();
            holdTimer = holdDuration;
            hasArrived = false;
        }

        public void Update()
        {
            if (baitObject == null)
            {
                hasBaitTarget = false;
            }
            
            if (!hasBaitTarget)
            {
                ReturnToWander();
                return;
            }

            if (!hasArrived && IsAtBait())
            {
                hasArrived = true;
            }

            if (!hasArrived)
            {
                return;
            }

            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                ReturnToWander();
            }
        }

        public void FixedUpdate()
        {
            if (!hasBaitTarget || hasArrived)
            {
                return;
            }

            Vector3 dir = NavSteering.GetSteeringDirection(ctx.NavAgent, ctx.Rigidbody.position, baitPosition, 0.1f);
            if (dir.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            ctx.Movement.MoveTowards(dir, moveSpeedMultiplier, turnResponsiveness, false);
        }

        public void Exit()
        {
            hasBaitTarget = false;
            hasArrived = false;
        }

        private bool IsAtBait()
        {
            return Vector3.Distance(ctx.Rigidbody.position, baitPosition) <= arrivalDistance;
        }

        private void ReturnToWander()
        {
            hasBaitTarget = false;
            hasArrived = false;

            if (returnState != null)
            {
                stateMachine.ChangeState(returnState);
            }
        }
    }
}






