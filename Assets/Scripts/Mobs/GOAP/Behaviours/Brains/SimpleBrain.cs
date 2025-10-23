using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using SIGGD.Mobs;
using Unity.VisualScripting;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;

namespace SIGGD.Goap.Behaviours
{
    public class SimpleBrain : BaseAgentBrain
    {
        private HungerBehaviour hungerBehaviour;

        protected override void Awake()
        {
            base.Awake();
            SetAgentType(MobIds.generic);
        }
        protected override void Start()
        {
            this.provider.RequestGoal<WanderGoal>(true);
        }

        protected override void OnActionEnd(IAction action)
        {
            this.DecideGoal();
        }
        protected override void OnNoActionFound(IGoalRequest request)
        {
            this.provider.RequestGoal<WanderGoal>(true);
        }
        private void DecideGoal()
        {
            //this.provider.RequestGoal<WanderGoal, GrowPackGoal, FollowAlphaGoal, DontStarveGoal>(true);
            this.provider.RequestGoal<WanderGoal, DontStarveGoal>(true);
        }
    }

}