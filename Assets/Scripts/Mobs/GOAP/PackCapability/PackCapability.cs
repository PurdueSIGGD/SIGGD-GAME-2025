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

            builder.AddGoal<StickTogetherGoal>()
                .AddCondition<InPack>(Comparison.GreaterThan, 0)
                .AddCondition<CloseToAlpha>(Comparison.SmallerThan, 1)
                .AddCondition<IsAlpha>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<FollowAlphaAction>()
                .AddCondition<InPack>(Comparison.GreaterThan, 0)
                .AddEffect<CloseToAlpha>(EffectType.Increase)
                .SetTarget<PackAlphaTarget>();

            builder.AddAction<JoinPackAction>()
                .AddCondition<InPack>(Comparison.SmallerThanOrEqual, 0)
                .AddEffect<InPack>(EffectType.Increase)
                .SetTarget<PackClosestTarget>();

            builder.AddMultiSensor<PackMultiSensor>();

            return builder.Build();
        }
    }

}

