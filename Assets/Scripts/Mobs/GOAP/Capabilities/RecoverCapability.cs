using UnityEngine;
using SIGGD.Goap.Sensors;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace SIGGD.Goap.Capabilities {

    public class RecoverCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("RecoverCapability");
            builder.AddGoal<HealGoal>()
                .AddCondition<CurrentHealth>(Comparison.GreaterThanOrEqual, 0)
                .SetBaseCost(-200);
            builder.AddAction<RestAction>()
                .AddEffect<CurrentHealth>(EffectType.Increase)
                .SetTarget<RestTarget>();
            builder.AddTargetSensor<RestTargetSensor>()
                .SetTarget<RestTarget>();

            /*
            builder.AddGoal<RegainStaminaGoal>()
                .AddCondition<CurrentStamina>(Comparison.GreaterThanOrEqual, 0);
            builder.AddAction<RestAction>()
                .AddEffect<CurrentHealth>(EffectType.Increase)
                .SetTarget<RestTarget>();
            builder.AddTargetSensor<RestTargetSensor>()
                .SetTarget<RestTarget>();
            */

            return builder.Build();
        }
    }
}