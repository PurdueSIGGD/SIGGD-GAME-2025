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
    public class SMPreyBrain : MonoBehaviour
    {
        private MobStateMachine stateMachine;
        private MobContext ctx;

        private WanderState wanderState;
        private FollowPackState followPackState;
        private FleeState fleeState;

        private void Awake()
        {
            ctx = new MobContext
            {
                Transform = transform,
                Rigidbody = GetComponent<Rigidbody>(),
                NavAgent = GetComponent<NavMeshAgent>(),
                Movement = GetComponent<Movement>(),
                AgentData = GetComponent<AgentData>(),
                Pack = GetComponent<PackBehavior>(),
                Perception = GetComponent<PerceptionManager>(),
                Smell = GetComponent<Smell>()
            };

            wanderState = new WanderState(ctx);
            followPackState = new FollowPackState(ctx);
            fleeState = new FleeState(ctx);

            stateMachine = new MobStateMachine();

            // initialize navmesh agent and validate that spawn position is within valid navmesh area
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
            bool success = NavMesh.SamplePosition(gameObject.transform.position, out NavMeshHit hit, 5f, navFilter);

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

                Debug.Log("Successfully initialized a Hyena");
            }
            else
            {
                Debug.Log("Failed to initlaize a Hyena");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            stateMachine.ChangeState(wanderState);
        }

        private void Update()
        {
            stateMachine.Update();
            EvaluateTransitions();
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        private void EvaluateTransitions()
        {
            var current = stateMachine.CurrentState;

            // Top priority: flee from predators
            if (InDanger() && current != fleeState)
            {
                stateMachine.ChangeState(fleeState);
                return;
            }

            // Fleeing — wait until safe
            if (current == fleeState)
            {
                if (fleeState.IsSafe)
                    stateMachine.ChangeState(wanderState);
                return;
            }

            // Follow pack when idle
            if (current == wanderState && HasPackToFollow())
            {
                stateMachine.ChangeState(followPackState);
                return;
            }

            // Pack lost alpha
            if (current == followPackState && !followPackState.HasValidAlpha)
            {
                stateMachine.ChangeState(wanderState);
                return;
            }
        }

        private bool HasPackToFollow()
        {
            if (ctx.Pack == null) return false;
            var pack = ctx.Pack.GetPack();
            if (pack == null) return false;
            var alpha = pack.GetAlpha();
            return alpha != null && alpha != ctx.Pack;
        }

        private bool InDanger()
        {
            return (ctx.Perception != null && ctx.Perception.predatorTargets.Count > 0) ||
                   (ctx.Smell != null && ctx.Smell.ClosestPred != null);
        }

        void OnDrawGizmos()
        {
            if (stateMachine == null) return;

            Handles.Label(transform.position + Vector3.up * 2f, stateMachine.CurrentState.GetType().Name);
        }
    }
}
