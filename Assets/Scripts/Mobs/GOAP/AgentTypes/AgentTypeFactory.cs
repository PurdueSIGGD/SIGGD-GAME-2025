using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Capabilities;
using UnityEngine;

namespace SIGGD.Goap.AgentTypes {
    public class AgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("BaseAgent");
            factory.AddCapability<HungerCapability>();
            factory.AddCapability<WanderCapability>();
            return factory.Build();
        }
    }
}
