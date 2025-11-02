using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using SIGGD.Goap.Sensors;
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
                .SetBaseCost(5);
            builder.AddAction<EatAction>()
                .AddEffect<Hunger>(EffectType.Decrease)
                .AddCondition<FoodCount>(Comparison.GreaterThanOrEqual, 1)
                .SetTarget<ClosestFood>()
                .SetStoppingDistance(1f)
                .SetBaseCost(10);
            builder.AddMultiSensor<FoodSensor>();
            //builder.AddWorldSensor<HungerSensor>()
            //    .SetKey<Hunger>();
           // builder.AddTargetSensor<HungerTargetSensor>()
            //    .SetTarget<HungerTarget>();

            return builder.Build();
        }
    }

}

