using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Capabilities;
using SIGGD.Mobs;
using UnityEngine;

namespace SIGGD.Goap.AgentTypes
{
    public class HyenaTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder(MobIds.hyena);
            factory.AddCapability<WanderCapability>();
            factory.AddCapability<HungerCapability>();
            factory.AddCapability<PackCapability>();
            factory.AddCapability<HuntPreyCapability>();
            factory.AddCapability<AttackPlayerCapability>();
            return factory.Build();
        }
    }
}
