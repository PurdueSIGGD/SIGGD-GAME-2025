using UnityEngine;
using SIGGD.Goap.Sensors;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace SIGGD.Goap.Capabilities {

    public class RestCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("RecoverCapability");
            builder.AddGoal<HealGoal>()
                .AddCondition<CurrentHealth>(Comparison.GreaterThanOrEqual, 0);
            builder.AddAction<RestAction>()
                .AddEffect<CurrentHealth>(EffectType.Increase)
                .SetTarget<SafetyTarget>();
            builder.AddTargetSensor<SafetyTargetSensor>()
                .SetTarget<SafetyTarget>();
            return builder.Build();
        }
    }
}