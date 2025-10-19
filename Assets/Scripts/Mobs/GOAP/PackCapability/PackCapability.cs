using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using SIGGD.Goap.Sensors;
using UnityEngine;

namespace SIGGD.Goap.Capabilities
{
    public class PackCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("PackCapability");

            builder.AddGoal<GrowPackGoal>()
                .AddCondition<InPack>(Comparison.GreaterThan, 0)
                .AddCondition<PackSize>(Comparison.GreaterThanOrEqual, int.MaxValue);
            builder.AddAction<JoinPackAction>()
                .AddEffect<InPack>(EffectType.Increase)
                .AddEffect<PackSize>(EffectType.Increase)
                .SetTarget<PackClosestTarget>()
                .SetStoppingDistance(2f);

            builder.AddGoal<FollowAlphaGoal>()
                .AddCondition<CloseToAlpha>(Comparison.GreaterThan, 0);
            builder.AddAction<FollowAlphaAction>()
                .AddCondition<InPack>(Comparison.GreaterThan, 0)
                .AddEffect<CloseToAlpha>(EffectType.Increase)
                .SetTarget<PackAlphaTarget>()
                .SetStoppingDistance(2f);

            builder.AddMultiSensor<PackMultiSensor>();

            return builder.Build();
        }
    }

}

