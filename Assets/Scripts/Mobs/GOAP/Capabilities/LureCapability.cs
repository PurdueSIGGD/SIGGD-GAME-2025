using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using SIGGD.Goap.Sensors;
using UnityEngine;

namespace SIGGD.Goap.Capabilities
{
    public class LureCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("LureCapability");
            builder.AddGoal<FindLureGoal>()
                .AddCondition<IsLureNearby>(Comparison.GreaterThan, 0);
            builder.AddAction<PursueLureAction>()
                .AddEffect<IsLureNearby>(EffectType.Increase)
                .SetTarget<LureTarget>()
                .SetStoppingDistance(4f);
            builder.AddTargetSensor<LureTargetSensor>()
                .SetTarget<LureTarget>();

            return builder.Build();
        }
    }

}

