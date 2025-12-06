using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using SIGGD.Goap.Sensors;
using UnityEngine;

namespace SIGGD.Goap.Capabilities
{
    public class AttackPlayerCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("AttackPlayerCapability");
            builder.AddGoal<KillPlayerGoal>()
                .AddCondition<PlayerHealth>(Comparison.SmallerThanOrEqual, 1)
                .SetBaseCost(10);
            builder.AddAction<AttackPlayerAction>()
                .AddEffect<PlayerHealth>(EffectType.Decrease)
                .SetTarget<PlayerLocation>()
                .SetStoppingDistance(12);
            builder.AddTargetSensor<PlayerTargetSensor>()
                .SetTarget<PlayerLocation>();

            return builder.Build();
        }
    }

}

