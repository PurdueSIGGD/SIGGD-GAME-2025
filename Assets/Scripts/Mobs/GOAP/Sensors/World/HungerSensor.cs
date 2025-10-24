using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Behaviours;
using UnityEngine;

namespace SIGGD.Goap.Sensors
{
    public class HungerSensor : LocalWorldSensorBase
    {
        public override void Created()
        {

        }

        public override void Update()
        {

        }
        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            return (int)references.GetCachedComponent<HungerBehaviour>().hunger;
        }

    }
}
