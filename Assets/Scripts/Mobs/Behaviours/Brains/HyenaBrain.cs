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
        private HungerBehaviour HungerBehaviour;
        private AgentHuntBehaviour HuntBehaviour;
        private HyenaAttackManager HyenaAttackManager;
        private PerceptionManager PerceptionManager;
        protected override void Awake()
        {
            this.goap = FindFirstObjectByType<GoapBehaviour>();
            this.agent = this.GetComponent<AgentBehaviour>();
            this.provider = this.GetComponent<GoapActionProvider>();
            this.provider.AgentType = this.goap.GetAgentType(MobIds.hyena);
            HuntBehaviour = this.GetComponent<AgentHuntBehaviour>();
            HungerBehaviour = this.GetComponent<HungerBehaviour>();
            HyenaAttackManager = this.GetComponent<HyenaAttackManager>();
            PerceptionManager = this.GetComponent<PerceptionManager>();
        }
        protected override void Start()
        {
            this.provider.RequestGoal<WanderGoal>(true);
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
            // If lunging then ignore selecting a new goal
            if (HyenaAttackManager.isLunging) return;

            if (this.provider.CurrentPlan == null)
            {
                this.provider.RequestGoal<WanderGoal, DontStarveGoal, GrowPackGoal>(true);
                return;
            }

            if (HungerBehaviour.hunger > 50 && this.provider.CurrentPlan.Goal is not DontStarveGoal)
            {
                this.provider.RequestGoal<DontStarveGoal, WanderGoal>(true);
                return;
            } 
        }
        protected override void OnNoActionFound(IGoalRequest request)
        {
            // If hunting or lunging then ignore selecting a new goal
            if (HyenaAttackManager.isLunging) return;
            if (HuntBehaviour.currentTargetOfHunt != null) return;
            
            if (this.provider.CurrentPlan == null)
            {
                this.provider.RequestGoal<DontStarveGoal, WanderGoal, GrowPackGoal>(true);
                return;
            }

            if (HungerBehaviour.hunger > 50 && this.provider.CurrentPlan.Goal is not DontStarveGoal)
            {
                //this.provider.RequestGoal<DontStarveGoal, FollowAlphaGoal, StickTogetherGoal, WanderGoal>(true);
                this.provider.RequestGoal<DontStarveGoal, WanderGoal>(true);
            }
            else
            {
                //this.provider.RequestGoal<FollowAlphaGoal, StickTogetherGoal, WanderGoal>(true);
                this.provider.RequestGoal<WanderGoal>(true);
            }
        }
        protected override void OnActionStart(IAction action)
        {
            // Plays SFX when detecting prey
            if (this.provider.CurrentPlan.Goal is KillPlayerGoal || this.provider.CurrentPlan.Action is KillPreyAction)
            {
                if (AudioManager.Instance)
                {
                    AudioManager.Instance.PlayOneShot(FMODEvents.Instance.soundEvents["HyenaOnNoticeSFX"].ToSafeString(), transform.position);
                }
            }
        }
        // Action for smell for when prey detected 
        void PlayerDetected(Transform player)
        {
            if (this.provider.CurrentPlan == null || (this.provider.CurrentPlan.Goal is not KillPlayerGoal))
            {
                this.provider.RequestGoal<KillPlayerGoal, DontStarveGoal>(true);
            }
            this.provider.ResolveAction();
        }
    }
}