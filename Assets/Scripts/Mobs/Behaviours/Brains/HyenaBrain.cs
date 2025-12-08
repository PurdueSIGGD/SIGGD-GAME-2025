using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using Unity.VisualScripting;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using SIGGD.Mobs.PackScripts;
using SIGGD.Goap;
using SIGGD.Mobs.Hyena;

namespace SIGGD.Mobs
{
    public class HyenaBrain : BaseAgentBrain
    {
        private Movement Movement;
        private HungerBehaviour HungerBehaviour;
        private AgentHuntBehaviour HuntBehaviour;
        private PackBehavior PackBehaviour;
        private HyenaAttackManager HyenaAttackManager;
        private PerceptionManager PerceptionManager;
        protected override void Awake()
        {
            this.goap = FindFirstObjectByType<GoapBehaviour>();
            this.agent = this.GetComponent<AgentBehaviour>();
            this.provider = this.GetComponent<GoapActionProvider>();
            this.provider.AgentType = this.goap.GetAgentType(MobIds.hyena);
            Movement = this.GetComponent<Movement>();
            HuntBehaviour = this.GetComponent<AgentHuntBehaviour>();
            HungerBehaviour = this.GetComponent<HungerBehaviour>();
            PackBehaviour = this.GetComponent<PackBehavior>();
            HyenaAttackManager = this.GetComponent<HyenaAttackManager>();
            PerceptionManager = this.GetComponent<PerceptionManager>();
        }
        protected override void Start()
        {
            //Start by requesting wander goal by default
            this.provider.RequestGoal<WanderGoal>(true);
            //Distance multiplier based off speed should match the hyenas speed
            this.provider.SetDistanceMultiplierSpeed(6f);
        }
        private void Update()
        {
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            PerceptionManager.OnPlayerDetected += PlayerDetected;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            PerceptionManager.OnPlayerDetected -= PlayerDetected;
        }

        protected override void OnActionEnd(IAction action)
        {
            // If it is lunging then ignore goap actions
            if (HyenaAttackManager.isLunging) return;
            // If current plan is null or hyena is hungry then reresolve goal by requesting again
            if (this.provider.CurrentPlan == null)
            {
                this.provider.RequestGoal<WanderGoal, DontStarveGoal, GrowPackGoal, FollowAlphaGoal, StickTogetherGoal>(true);
                return;
            } else if (HungerBehaviour.hunger > 50)
            {
                this.provider.RequestGoal<WanderGoal, DontStarveGoal, GrowPackGoal>(false);
                return;
            }               
        }
        protected override void OnNoActionFound(IGoalRequest request)
        {
            // If it is lunging then ignore goap actions
            if (HyenaAttackManager.isLunging) return;
            // If hyena is hungry and current not trying to resolve the hunger then request to resolve hunger
            if (this.provider.CurrentPlan == null || HungerBehaviour.hunger > 50 && this.provider.CurrentPlan.Goal is not DontStarveGoal)
            {
                this.provider.RequestGoal<DontStarveGoal, WanderGoal>(true);
            } else
            {
                this.provider.RequestGoal<WanderGoal>(true);
            }
        }
        protected override void OnActionStart(IAction action)
        {
            // When the action is starting if the goal is to attack a player then play sound effect
            if (this.provider.CurrentPlan.Goal is KillPlayerGoal)
            {
                if (AudioManager.Instance)
                {
                    AudioManager.Instance.PlayOneShot(FMODEvents.Instance.soundEvents["HyenaOnNoticeSFX"].ToSafeString(), transform.position);
                }
            }
        }
        void PlayerDetected(Transform player)
        {
            // When the player is detected set goal to killing player goal if not hungry
            // If hyena is hungry then resolve between hunger and killing player
            if (this.provider.CurrentPlan == null || (this.provider.CurrentPlan.Goal is not KillPlayerGoal && HungerBehaviour.hunger < 150))
            {
                this.provider.RequestGoal<KillPlayerGoal>(true);
            } else
            {
                this.provider.RequestGoal<KillPlayerGoal, DontStarveGoal>(true);
            }
        }
    }
}