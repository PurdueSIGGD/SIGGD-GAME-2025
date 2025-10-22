using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using SIGGD.Mobs;
using Unity.VisualScripting;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using SIGGD.Goap.Sensors;

namespace SIGGD.Goap.Behaviours
{
    public class HyenaBrain : BaseAgentBrain
    {
        private AgentMoveBehaviour AgentMoveBehaviour;
        private HungerBehaviour HungerBehaviour;
        private AgentHuntBehaviour HuntBehaviour;
        protected override void Awake()
        {
            this.goap = FindFirstObjectByType<GoapBehaviour>();
            this.agent = this.GetComponent<AgentBehaviour>();
            this.provider = this.GetComponent<GoapActionProvider>();
            this.provider.AgentType = this.goap.GetAgentType(MobIds.hyena);
            AgentMoveBehaviour = this.GetComponent<AgentMoveBehaviour>();
            HuntBehaviour = this.GetComponent<AgentHuntBehaviour>();
            HungerBehaviour = this.GetComponent<HungerBehaviour>();
        }
        protected override void Start()
        {
            this.provider.RequestGoal<WanderGoal, DontStarveGoal>(true);
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

            if (HungerBehaviour.hunger > 50)
            {
                this.provider.RequestGoal<DontStarveGoal>(false);
            }
        }
        protected override void OnNoActionFound(IGoalRequest request)
        {
            if (HuntBehaviour.currentTargetOfHunt != null)
                return;
            if (this.provider.CurrentPlan.Goal is DontStarveGoal)
                return;
            if (this.provider.CurrentPlan.Goal is not KillPlayerGoal)
            {
                AgentMoveBehaviour.DisableSprint();
                this.provider.RequestGoal<WanderGoal>(false);
            }
        }
        protected override void OnActionStart(IAction action)
        {
            if (this.provider.CurrentPlan.Action is KillPreyAction)
                AgentMoveBehaviour.EnableSprint();
            if (this.provider.CurrentPlan.Goal is KillPlayerGoal)
            {
                AgentMoveBehaviour.EnableSprint();
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
                AgentMoveBehaviour.EnableSprint();
                this.provider.RequestGoal<KillPlayerGoal>(true);
            }
        }
    }

}