using SIGGD.Goap.Sensors;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace SIGGD.Goap.Capabilities
{
    public class HungerCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("HungerCapability");

            builder.AddGoal<DontStarveGoal>()
                .AddCondition<Hunger>(Comparison.SmallerThanOrEqual, 0)
                .SetBaseCost(100);
            builder.AddAction<EatAction>()
            //  .AddCondition<Food>
                .AddEffect<Hunger>(EffectType.Decrease)
                .SetRequiresTarget(false);
            return builder.Build();
        }
    }

}

