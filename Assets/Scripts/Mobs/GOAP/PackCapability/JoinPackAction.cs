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
        // This method is called every frame while the action is running
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            PackBehavior nearestNeighbor = data.PackBehaviour.FindNearbyNeighbor(excludePack: true);
            if (nearestNeighbor == null || !PackManager.CanJoin(data.PackBehaviour, nearestNeighbor))
                return ActionRunState.Stop;

            float distance = PackBehavior.CalculateDistanceVector(data.PackBehaviour, nearestNeighbor).magnitude;
            if (distance < data.PackBehaviour.Data.JoinPackRange)
            {
                data.PackBehaviour.JoinPack(nearestNeighbor);
                return ActionRunState.Completed;
            }
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