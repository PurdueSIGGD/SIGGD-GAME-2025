using SIGGD.Mobs.Hyena;
using SIGGD.Mobs.PackScripts;
using SIGGD.Mobs.StateMachine.States;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs.StateMachine
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(AgentData))]
    [RequireComponent(typeof(HungerBehaviour))]
    [RequireComponent(typeof(PackBehavior))]
    [RequireComponent(typeof(PerceptionManager))]
    [RequireComponent(typeof(HyenaAttackManager))]
    [RequireComponent(typeof(AgentHuntBehaviour))]
    [RequireComponent(typeof(Smell))]
    public class SMHyenaBrain : MonoBehaviour
    {
        private MobStateMachine stateMachine;
        private MobContext ctx;

        private WanderState wanderState;
        private SeekFoodState seekFoodState;
        private FollowPackState followPackState;
        private ChasePlayerState chasePlayerState;
        private AttackPlayerState attackPlayerState;
        private ChasePreyState chasePreyState;
        private AttackPreyState attackPreyState;

        [SerializeField] private float hungerThreshold = 50f;

        private void Awake()
        {
            ctx = new MobContext
            {
                Transform = transform,
                Rigidbody = GetComponent<Rigidbody>(),
                NavAgent = GetComponent<NavMeshAgent>(),
                Movement = GetComponent<Movement>(),
                AgentData = GetComponent<AgentData>(),
                Hunger = GetComponent<HungerBehaviour>(),
                Pack = GetComponent<PackBehavior>(),
                Perception = GetComponent<PerceptionManager>(),
                AttackManager = GetComponent<HyenaAttackManager>(),
                HuntBehaviour = GetComponent<AgentHuntBehaviour>(),
                Smell = GetComponent<Smell>()
            };

            wanderState = new WanderState(ctx);
            seekFoodState = new SeekFoodState(ctx);
            followPackState = new FollowPackState(ctx);
            chasePlayerState = new ChasePlayerState(ctx);
            attackPlayerState = new AttackPlayerState(ctx);
            chasePreyState = new ChasePreyState(ctx);
            attackPreyState = new AttackPreyState(ctx);

            stateMachine = new MobStateMachine();
        }

        private void OnEnable()
        {
            if (ctx.Perception != null)
                ctx.Perception.OnPlayerDetected += OnPlayerDetected;
        }

        private void OnDisable()
        {
            if (ctx.Perception != null)
                ctx.Perception.OnPlayerDetected -= OnPlayerDetected;
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
            bool isAttacking = current == attackPlayerState || current == attackPreyState;

            // While lunging, do not interrupt the attack
            if (ctx.AttackManager != null && ctx.AttackManager.isLunging)
            {
                if (!isAttacking)
                    stateMachine.ChangeState(attackPlayerState);
                return;
            }

            // Player attack finished — decide next state
            if (current == attackPlayerState && attackPlayerState.IsAttackFinished)
            {
                if (PlayerVisible())
                    stateMachine.ChangeState(chasePlayerState);
                else if (PreyVisible())
                    stateMachine.ChangeState(chasePreyState);
                else
                    stateMachine.ChangeState(wanderState);
                return;
            }

            // Prey attack finished — decide next state
            if (current == attackPreyState && attackPreyState.IsAttackFinished)
            {
                if (PlayerVisible())
                    stateMachine.ChangeState(chasePlayerState);
                else if (PreyVisible())
                    stateMachine.ChangeState(chasePreyState);
                else
                    stateMachine.ChangeState(wanderState);
                return;
            }

            // Chasing player
            if (current == chasePlayerState)
            {
                if (chasePlayerState.HasLostPlayer)
                {
                    stateMachine.ChangeState(wanderState);
                    return;
                }
                if (chasePlayerState.IsInAttackRange)
                {
                    stateMachine.ChangeState(attackPlayerState);
                    return;
                }
                return;
            }

            // Chasing prey — player detection interrupts
            if (current == chasePreyState)
            {
                if (PlayerVisible())
                {
                    stateMachine.ChangeState(chasePlayerState);
                    return;
                }
                if (chasePreyState.HasLostPrey)
                {
                    stateMachine.ChangeState(wanderState);
                    return;
                }
                if (chasePreyState.IsInAttackRange)
                {
                    attackPreyState.SetTarget(chasePreyState.CurrentTarget);
                    stateMachine.ChangeState(attackPreyState);
                    return;
                }
                return;
            }

            // If player is visible, chase (highest combat priority)
            if (PlayerVisible() && current != chasePlayerState && !isAttacking)
            {
                stateMachine.ChangeState(chasePlayerState);
                return;
            }

            // If prey is visible and not already in a combat state, chase prey
            if (PreyVisible() && current != chasePreyState && !isAttacking && current != chasePlayerState)
            {
                stateMachine.ChangeState(chasePreyState);
                return;
            }

            // Hungry — seek food
            if (ctx.Hunger != null && ctx.Hunger.hunger > hungerThreshold &&
                current != seekFoodState)
            {
                stateMachine.ChangeState(seekFoodState);
                return;
            }

            // Done eating or no food found
            if (current == seekFoodState && !seekFoodState.HasFood)
            {
                stateMachine.ChangeState(wanderState);
                return;
            }

            // Follow pack when not busy
            if (current == wanderState && HasPackToFollow())
            {
                stateMachine.ChangeState(followPackState);
                return;
            }

            // Pack lost alpha or already close
            if (current == followPackState && !followPackState.HasValidAlpha)
            {
                stateMachine.ChangeState(wanderState);
                return;
            }
        }

        private void OnPlayerDetected(Transform player)
        {
            var current = stateMachine.CurrentState;
            if (current != chasePlayerState && current != attackPlayerState)
            {
                stateMachine.ChangeState(chasePlayerState);
            }
        }

        private bool PlayerVisible()
        {
            bool canSee = ctx.Perception != null &&
                          ctx.Perception.CanSeePlayer &&
                          ctx.Perception.PlayerTarget != null;
            bool canSmell = ctx.Smell != null && ctx.Smell.PlayerTarget != null;
            return canSee || canSmell;
        }

        private bool PreyVisible()
        {
            bool canSee = ctx.Perception != null &&
                          ctx.Perception.preyTargets != null &&
                          ctx.Perception.preyTargets.Count > 0;
            bool canSmell = ctx.Smell != null && ctx.Smell.ClosestPrey != null;
            return canSee || canSmell;
        }

        private bool HasPackToFollow()
        {
            if (ctx.Pack == null) return false;
            var pack = ctx.Pack.GetPack();
            if (pack == null) return false;
            var alpha = pack.GetAlpha();
            return alpha != null && alpha != ctx.Pack;
        }
    }
}
