using System;
using UnityEngine;
using Object = UnityEngine.Object;

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

        private float holdDuration;
        private float holdTimer;
        private bool hasBaitTarget;
        private bool hasArrived;
        private bool eating;
        public bool returnToSender;
        private bool idle;

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
        public void Configure(GameObject baitObject, float duration)
        {
            this.baitObject = baitObject;
            holdDuration = Mathf.Max(0f, duration);
            hasBaitTarget = true;
            hasArrived = false;
            returnToSender = false;
            eating = false;
            idle = false;
        }

        public void Enter()
        {
            Debug.Log("[BaitedState] Entering state: " + stateMachine.GetType().Name);
            ctx.Movement.DisableSprint();
            holdTimer = holdDuration;
            hasArrived = false;
            returnToSender = false;
            eating = false;
            idle = false;
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

            eating = true;
            
            ctx.Rigidbody.linearVelocity = Vector3.zero;
            

            if (idle == false)
            {
                idle = true;
                ctx.Animator.SetTrigger(Animator.StringToHash("Return To Idle"));
            }
            
            Debug.Log("[BaitedState] Holding at bait: " + baitObject.name + " with " + holdTimer + " seconds remaining.");

            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                Object.Destroy(baitObject);
                ReturnToWander();
            }
        }

        public void FixedUpdate()
        {
            if (!hasBaitTarget || hasArrived || eating)
            {
                return;
            }

            Vector3 dir;
            try
            {
                dir = NavSteering.GetSteeringDirection(ctx.NavAgent, ctx.Rigidbody.position,
                    baitObject?.transform.position ?? ctx.Rigidbody.position, 0.1f);

            }
            catch (Exception e)
            {
                return;
            }
            
            if (dir.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            ctx.Movement.MoveTowards(dir, moveSpeedMultiplier, turnResponsiveness, true);
        }

        public void Exit()
        {
            Debug.Log("[BaitedState] Exiting state: " + stateMachine.GetType().Name);
            hasBaitTarget = false;
            hasArrived = false;
            returnToSender = false;
            eating = false;
            idle = false;
        }

        private bool IsAtBait()
        {
            if (baitObject == null) return false;
            Debug.Log("[BaitedState] Checking arrival at bait: " + baitObject.name + " with distance: " + Vector3.Distance(ctx.Rigidbody.position, baitObject.transform.position) + " and threshold: " + arrivalDistance);
            return Vector3.Distance(ctx.Rigidbody.position, baitObject.transform.position) <= arrivalDistance;
        }

        private void ReturnToWander()
        {
            hasBaitTarget = false;
            hasArrived = false;

            Debug.Log("[BaitedState] Returning to wander state: " + returnState.GetType().Name);
            returnToSender = true;
        }
    }
}






