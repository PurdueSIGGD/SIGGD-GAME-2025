using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace SIGGD.Goap.Sensors
{
    public class PlayerTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {

        }
    }


}
