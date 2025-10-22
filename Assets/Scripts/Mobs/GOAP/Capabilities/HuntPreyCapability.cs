using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using SIGGD.Goap.Sensors;
using UnityEngine;

namespace SIGGD.Goap.Capabilities
{
    public class HuntPreyCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("HuntPreyCapability");
            builder.AddAction<KillPreyAction>()
                .SetBaseCost(30)
                .AddEffect<FoodCount>(EffectType.Increase)
                .SetTarget<ClosestPrey>()
                .SetStoppingDistance(2);
            builder.AddTargetSensor<ClosestPreyTargetSensor>()
                .SetTarget<ClosestPrey>();

            return builder.Build();
        }
    }

}

