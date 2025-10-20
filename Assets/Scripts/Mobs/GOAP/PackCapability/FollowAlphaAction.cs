using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using SIGGD.Goap.Behaviours;
using SIGGD.Mobs.PackScripts;
using UnityEngine;

namespace SIGGD.Goap
{
    public class FollowAlphaAction : GoapActionBase<FollowAlphaAction.Data>
    {
        public override void Start(IMonoAgent agent, Data data)
        {
            base.Start(agent, data);
            data.AlphaTarget = data.PackBehaviour.GetPack().GetAlpha();
        }
        // This method is called every frame while the action is running
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            return ActionRunState.Completed;
        }
        public override bool IsValid(IActionReceiver agent, Data data)
        {
            return data.PackBehaviour.GetPack() != null &&
                data.AlphaTarget != null &&
                !data.PackBehaviour.CheckLeaveRange(data.AlphaTarget);
        }
        public override void Stop(IMonoAgent agent, Data data)
        {
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            [GetComponent]
            public PackBehavior PackBehaviour { get; set; }
            public ITarget Target { get; set; }
            public PackBehavior AlphaTarget { get; set; }
        }
    }
}