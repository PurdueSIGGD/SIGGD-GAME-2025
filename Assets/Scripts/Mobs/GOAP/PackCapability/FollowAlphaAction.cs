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

        // This method is called every frame while the action is running
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.PackBehaviour.GetPack().GetAlpha() == null)
                return ActionRunState.Completed;

            if (data.PackBehaviour.GetRawAlphaPositionDiff().magnitude < data.PackBehaviour.Data.CloseEnoughToAlphaDist)
                return ActionRunState.Completed;
            return ActionRunState.Continue;
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            [GetComponent]
            public PackBehavior PackBehaviour { get; set; }
            public ITarget Target { get; set; }
        }
    }
}