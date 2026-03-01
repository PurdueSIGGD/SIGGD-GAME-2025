using SIGGD.Mobs.PackScripts;
using SIGGD.Mobs.StateMachine.States;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs.StateMachine
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(AgentData))]
    [RequireComponent(typeof(PackBehavior))]
    [RequireComponent(typeof(PerceptionManager))]
    [RequireComponent(typeof(Smell))]
    public abstract class MobBrainBase : MonoBehaviour
    {
        protected MobStateMachine stateMachine;
        protected MobContext ctx;

        protected WanderState wanderState;
        protected FollowPackState followPackState;

        public MobStateMachine StateMachine => stateMachine;
        public MobContext Context => ctx;

        /// <summary>
        /// Display name used in debug logs (e.g. "Hyena", "Prey").
        /// </summary>
        protected abstract string MobName { get; }

        /// <summary>
        /// Build the <see cref="MobContext"/> with all required component references.
        /// </summary>
        protected abstract MobContext BuildContext();

        /// <summary>
        /// Create mob-specific states beyond the shared wander/followPack states.
        /// Called after <see cref="BuildContext"/> during <c>Awake</c>.
        /// </summary>
        protected abstract void InitializeStates();

        /// <summary>
        /// Evaluates the current state and determines whether any transitions should occur based on defined conditions.
        /// </summary>
        /// <remarks>Override this method in a derived class to implement custom logic for evaluating and
        /// triggering state transitions. This method is typically called as part of a state machine update
        /// cycle.</remarks>
        protected abstract void EvaluateTransitions();

        protected virtual void Awake()
        {
            ctx = BuildContext();

            wanderState = new WanderState(ctx);
            followPackState = new FollowPackState(ctx);
            InitializeStates();

            stateMachine = new MobStateMachine();

            InitializeNavMesh();
        }

        protected virtual void Start()
        {
            stateMachine.ChangeState(wanderState);
        }

        protected virtual void Update()
        {
            stateMachine.Update();
            EvaluateTransitions();
        }

        protected virtual void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        public bool HasPackToFollow()
        {
            if (ctx.Pack == null) return false;
            var pack = ctx.Pack.GetPack();
            if (pack == null) return false;
            var alpha = pack.GetAlpha();
            return alpha != null && alpha != ctx.Pack;
        }

        /// <summary>
        /// Shared NavMesh + Rigidbody bootstrap.
        /// </summary>
        private void InitializeNavMesh()
        {
            NavMeshAgent navAgent = ctx.NavAgent;
            navAgent.updatePosition = false;
            navAgent.updateRotation = false;

            NavMeshQueryFilter navFilter = new NavMeshQueryFilter
            {
                agentTypeID = ctx.NavAgent.agentTypeID,
                areaMask = NavMesh.AllAreas
            };
            if (ctx.AgentData != null && ctx.AgentData.filter.areaMask != 0)
                navFilter = ctx.AgentData.filter;

            bool success = NavMesh.SamplePosition(
                gameObject.transform.position, out NavMeshHit hit, 5f, navFilter);

            if (success)
            {
                transform.position = hit.position;
                navAgent.Warp(hit.position);
                navAgent.nextPosition = hit.position;
                navAgent.ResetPath();
                navAgent.isStopped = false;

                Rigidbody rb = ctx.Rigidbody;
                if (rb != null)
                {
                    if (!rb.isKinematic)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.position = hit.position;
                }

                Debug.Log($"Successfully initialized a {MobName}");
            }
            else
            {
                Debug.Log($"Failed to initialize a {MobName}");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (stateMachine == null) return;

            Handles.Label(
                transform.position + Vector3.up * 2f,
                stateMachine.CurrentState.GetType().Name);
        }
    }
}