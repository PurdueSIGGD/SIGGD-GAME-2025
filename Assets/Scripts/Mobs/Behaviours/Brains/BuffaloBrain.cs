using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using Unity.VisualScripting;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using SIGGD.Goap;

namespace SIGGD.Mobs
{
    public class BuffaloBrain : BaseAgentBrain
    {
        protected override void Awake()
        {
            base.Awake();
            SetAgentType(MobIds.buffalo);
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
            this.provider.RequestGoal<WanderGoal, FollowAlphaGoal, GrowPackGoal>(true);
        }
    }

}