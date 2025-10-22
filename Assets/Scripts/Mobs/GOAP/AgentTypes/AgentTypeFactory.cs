using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Capabilities;
using SIGGD.Mobs;
using UnityEngine;

namespace SIGGD.Goap.AgentTypes
{
    public class AgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder(MobIds.generic);
            factory.AddCapability<HungerCapability>();
            factory.AddCapability<WanderCapability>();
            factory.AddCapability<HuntPreyCapability>();
            factory.AddCapability<PackCapability>();
            return factory.Build();
        }
    }
}
