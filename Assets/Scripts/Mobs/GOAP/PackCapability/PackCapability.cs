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
            return builder.Build();
        }
    }

}

