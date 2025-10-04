using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Capabilities;
using SIGGD.Mobs;
using UnityEngine;

namespace SIGGD.Goap.AgentTypes
{
    public class PreyTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder(MobIds.prey);
            factory.AddCapability<HungerCapability>();
            factory.AddCapability<WanderCapability>();
            return factory.Build();
        }
    }
}
