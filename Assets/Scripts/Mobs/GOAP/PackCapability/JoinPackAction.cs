using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using SIGGD.Goap.Behaviours;
using SIGGD.Mobs.PackScripts;
using UnityEngine;

namespace SIGGD.Goap
{
    public class JoinPackAction : GoapActionBase<JoinPackAction.Data>
    {
        public override void Start(IMonoAgent agent, Data data)
        {
            base.Start(agent, data);
            data.PackTarget = data.PackBehaviour.GetLastNearestNeighbor();
        }
        // This method is called every frame while the action is running
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.PackBehaviour.TryJoinPack(data.PackTarget);
            return ActionRunState.Completed;
        }
        public override bool IsValid(IActionReceiver agent, Data data)
        {
            float distance = PackBehavior.CalculateDistanceVector(
                data.PackBehaviour,
                data.PackTarget).magnitude;
            return distance < data.PackBehaviour.Data.LeavePackRange &&
                data.PackTarget != null &&
                data.Target != null && data.Target.IsValid() &&
                PackManager.CanJoin(data.PackBehaviour, data.PackTarget);
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            [GetComponent]
            public PackBehavior PackBehaviour { get; set; }
            public ITarget Target { get; set; }
            public PackBehavior PackTarget { get; set; }
        }
    }
}