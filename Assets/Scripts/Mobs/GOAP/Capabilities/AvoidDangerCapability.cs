using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using SIGGD.Goap.Sensors;
using UnityEngine;

namespace SIGGD.Goap.Capabilities
{
    public class AvoidDangerCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("AvoidDangerCapability");

            builder.AddGoal<AvoidDangerGoal>()
                .AddCondition<Danger>(Comparison.SmallerThanOrEqual, 0)
                .SetBaseCost(-50);
            builder.AddAction<GetToSafetyAction>()
                .AddEffect<Danger>(EffectType.Decrease)
                .SetTarget<SafetyTarget>()
                .SetStoppingDistance(2f)
                .SetBaseCost(-40);
            builder.AddTargetSensor<RestTargetSensor>()
               .SetTarget<SafetyTarget>();
            builder.AddWorldSensor<DangerSensor>()
                .SetKey<Danger>();

            return builder.Build();
        }
    }

}

