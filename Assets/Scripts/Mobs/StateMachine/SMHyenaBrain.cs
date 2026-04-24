using System;
using System.Collections;
using FMOD;
using FMOD.Studio;
using SIGGD.Mobs.Hyena;
using SIGGD.Mobs.StateMachine.States;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs.StateMachine
{
    [RequireComponent(typeof(HungerBehaviour))]
    [RequireComponent(typeof(HyenaAttackManager))]
    public class SMHyenaBrain : MobBrainBase
    {
        public ChasePlayerState ChasePlayer => chasePlayerState;
        public ChasePreyState ChasePrey => chasePreyState;

        private float knockbackDuration = 2f;
        private float knockbackForce = 20f;
        
        private SeekFoodState seekFoodState;
        private ChasePlayerState chasePlayerState;
        private AttackPlayerState attackPlayerState;
        private ChasePreyState chasePreyState;
        private AttackPreyState attackPreyState;
        private ParriedState parriedState;

        [SerializeField] private float hungerThreshold = 50f;
        [SerializeField] private Animator animator;

        [SerializeField] private GameObject deathModel;

        protected override string MobName => "Hyena";

        // Audio name
        private readonly string onNoticePlayerSound = "HyenaOnNotice";
        private readonly string passivePantSound = "HyenaPassivePant";
        private EventInstance passivePantEvent;

        protected override MobContext BuildContext()
        {
            return new MobContext
            {
                Transform = transform,
                Rigidbody = GetComponent<Rigidbody>(),
                NavAgent = GetComponent<NavMeshAgent>(),
                Movement = GetComponent<Movement>(),
                AgentData = GetComponent<AgentData>(),
                Hunger = GetComponent<HungerBehaviour>(),
                Pack = GetComponent<PackScripts.PackBehavior>(),
                Perception = GetComponent<PerceptionManager>(),
                AttackManager = GetComponent<HyenaAttackManager>(),
                Smell = GetComponent<Smell>(),
                Type = MobType.Hyena,
                Animator = animator
            };
        }

        protected override void InitializeStates()
        {
            seekFoodState = new SeekFoodState(ctx);
            chasePlayerState = new ChasePlayerState(ctx);
            attackPlayerState = new AttackPlayerState(ctx);
            chasePreyState = new ChasePreyState(ctx);
            attackPreyState = new AttackPreyState(ctx);
            parriedState = new ParriedState(ctx, knockbackForce, knockbackDuration);
        }

        private void OnEnable()
        {
            EntityHealthManager.OnDeath += HyenaDeath;
            
            if (ctx.Perception != null)
                ctx.Perception.OnPlayerDetected += OnPlayerDetected;
        }

        private void OnDisable()
        {
            EntityHealthManager.OnDeath -= HyenaDeath;
            
            if (ctx.Perception != null)
                ctx.Perception.OnPlayerDetected -= OnPlayerDetected;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            FMODEvents.Instance.GetEventInstance(passivePantSound, instance => { passivePantEvent = instance; });
        }

        protected override void Update()
        {
            base.Update();
            ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(transform.position, ctx.Rigidbody.linearVelocity, transform.forward, Vector3.up);
            passivePantEvent.set3DAttributes(attr);

            PLAYBACK_STATE playbackState;
            passivePantEvent.getPlaybackState(out playbackState);

            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                passivePantEvent.start();
            }
        }
        
        public void TryParry()
        {
            if (stateMachine.CurrentState == attackPlayerState && !attackPlayerState.IsAttackFinished)
            {
                StartCoroutine(PlayerInvincible(1));
                parriedState.SetDirection(ctx.Transform.forward);
                stateMachine.ChangeState(parriedState);
            }
        }

        IEnumerator PlayerInvincible(float time)
        {
            var playerID = PlayerID.Instance;

            playerID.playerHealth.SetInvincible(true);
            
            yield return new WaitForSeconds(time);
            
            playerID.playerHealth.SetInvincible(false);
        }

        protected override void EvaluateTransitions()
        {
            var current = stateMachine.CurrentState;
            bool isAttacking = current == attackPlayerState || current == attackPreyState;

            if (current == parriedState)
            {
                if (parriedState.finished)
                {
                    if (PlayerVisible())
                        stateMachine.ChangeState(chasePlayerState);
                    else if (PreyVisible())
                        stateMachine.ChangeState(chasePreyState);
                    else
                        stateMachine.ChangeState(wanderState);
                }                
                return;
            }

            if (current == baitedState)
            {
                if (baitedState.returnToSender)
                {
                    stateMachine.ChangeState(wanderState);
                }
                return;
            }

            // While lunging, do not interrupt the attack
            if (ctx.AttackManager != null && ctx.AttackManager.isLunging)
            {
                if (!isAttacking)
                {
                    AudioManager.Instance.PlayOneShotNoAsync(onNoticePlayerSound, transform.position);
                    stateMachine.ChangeState(attackPlayerState);
                }
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

        private void HyenaDeath(DamageContext context)
        {
            if (context.victim != gameObject) return;
            
            Instantiate(deathModel, transform.position, transform.rotation);
        }
    }
}
