using SIGGD.Goap.Sensors;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace SIGGD.Goap.Capabilities
{
    public class WanderCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("WanderCapability");

            builder.AddGoal<WanderGoal>()
                .AddCondition<IsWandering>(Comparison.GreaterThanOrEqual, 1)
                .SetBaseCost(30);
            builder.AddAction<WanderAction>()
                .AddEffect<IsWandering>(EffectType.Increase)
                .SetTarget<WanderTarget>()
                .SetStoppingDistance(2);
            builder.AddTargetSensor<WanderTargetSensor>()
                .SetTarget<WanderTarget>();
            return builder.Build();
        }
    }
}