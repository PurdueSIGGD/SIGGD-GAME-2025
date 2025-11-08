
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Mobs;
using System.Collections.Generic;
using UnityEngine;

namespace SIGGD.Goap.Sensors
{
    public class DangerSensor : LocalWorldSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame

        /*
        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            var hungerBehaviour = references.GetCachedComponent<>();

            if (hungerBehaviour == null)
                return false;

            return DangerLevel > threshold;
        }
        */
        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            var PreyBehaviour = references.GetCachedComponent<PreyBehaviour>();
            return PreyBehaviour.predatorCount;
        }
    }
}