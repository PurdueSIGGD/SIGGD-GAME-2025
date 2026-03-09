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
            builder.AddAction<TrackSmellAction>()
                .AddEffect<PlayerHealth>(EffectType.Decrease)
               // .SetTarget<SmellLure>()
                .SetStoppingDistance(12);
            builder.AddTargetSensor<PlayerTargetSensor>()
                .SetTarget<PlayerLocation>();

            return builder.Build();
        }
    }

}

