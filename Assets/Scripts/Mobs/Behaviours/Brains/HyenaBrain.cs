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
        private AgentMoveBehaviour AgentMoveBehaviour;
        private HungerBehaviour HungerBehaviour;
        private AgentHuntBehaviour HuntBehaviour;
        private PackBehavior PackBehaviour;
        private HyenaAttackManager HyenaAttackManager;
        protected override void Awake()
        {
            this.goap = FindFirstObjectByType<GoapBehaviour>();
            this.agent = this.GetComponent<AgentBehaviour>();
            this.provider = this.GetComponent<GoapActionProvider>();
            this.provider.AgentType = this.goap.GetAgentType(MobIds.hyena);
            AgentMoveBehaviour = this.GetComponent<AgentMoveBehaviour>();
            HuntBehaviour = this.GetComponent<AgentHuntBehaviour>();
            HungerBehaviour = this.GetComponent<HungerBehaviour>();
            PackBehaviour = this.GetComponent<PackBehavior>();
            HyenaAttackManager = this.GetComponent<HyenaAttackManager>();
        }
        protected override void Start()
        {
            this.provider.RequestGoal<WanderGoal>(true);
        }
        private void Update()
        {
          // if (this.provider.CurrentPlan == null) this.provider.RequestGoal<WanderGoal>();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            FieldOfView.OnPlayerDetected += PlayerDetected;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            FieldOfView.OnPlayerDetected -= PlayerDetected;
        }

        protected override void OnActionEnd(IAction action)
        {
            if (HyenaAttackManager.isLunging) return;
            if (this.provider.CurrentPlan == null)
            {
                this.provider.RequestGoal<WanderGoal, DontStarveGoal>(true);
                return;
            } else if (HungerBehaviour.hunger > 50)
            {
                this.provider.RequestGoal<WanderGoal, DontStarveGoal>(false);
                return;
            }               
            //this.provider.ResolveAction();
        }
        protected override void OnNoActionFound(IGoalRequest request)
        {
            if (HyenaAttackManager.isLunging) return;
            if (HuntBehaviour.currentTargetOfHunt != null)
                return;
            if (this.provider.CurrentPlan == null) return;
            if (HungerBehaviour.hunger > 50 && this.provider.CurrentPlan.Goal is not DontStarveGoal)
            {
                this.provider.RequestGoal<DontStarveGoal>(true);
            } else
            {
                this.provider.RequestGoal<WanderGoal>(true);
            }
            /*
            else if (this.provider.CurrentPlan.Goal is not KillPlayerGoal)
            {
                //AgentMoveBehaviour.DisableSprint();
                //this.provider.RequestGoal<FollowAlphaGoal, StickTogetherGoal>(false);
                this.provider.RequestGoal<WanderGoal>(true);
            }
            */
        }
        protected override void OnActionStart(IAction action)
        {
            if (this.provider.CurrentPlan.Action is KillPreyAction)
                //AgentMoveBehaviour.EnableSprint();
            if (this.provider.CurrentPlan.Goal is KillPlayerGoal)
            {
                //AgentMoveBehaviour.EnableSprint();
            }
        }
        private void DecideGoal()
        {
            this.provider.RequestGoal<WanderGoal>(true);
        }

        void PlayerDetected(Transform player)
        {
            if (this.provider.CurrentPlan.Goal is not KillPlayerGoal && HungerBehaviour.hunger < 50)
            {
               // AgentMoveBehaviour.EnableSprint();
                this.provider.RequestGoal<KillPlayerGoal>(true);
            } else
            {
                this.provider.RequestGoal<KillPlayerGoal, DontStarveGoal>(true);
            }
        }
    }

}