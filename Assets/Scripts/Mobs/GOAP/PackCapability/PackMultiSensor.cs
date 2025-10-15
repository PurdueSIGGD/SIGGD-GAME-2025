using System;
using System.Collections.Generic;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap;
using SIGGD.Goap.PackScripts;
using UnityEngine;

namespace CrashKonijn.Docs.GettingStarted.Sensors
{
    public class PackSensor : MultiSensorBase
    {
        // You must use the constructor to register all the sensors
        // This can also be called outside of the gameplay loop to validate the configuration
        public PackSensor()
        {
            this.AddLocalWorldSensor<DistanceFromAlpha>((agent, references) =>
            {
                var packBehavior = references.GetCachedComponent<PackBehavior>();
                return packBehavior.GetDistanceKey();
            });
            this.AddLocalWorldSensor<IsAlpha>((agent, references) =>
            {
                var packBehavior = references.GetCachedComponent<PackBehavior>();
                return packBehavior.GetIsAlphaKey();
            });
            this.AddLocalTargetSensor<PackAlphaTarget>((agent, references, target) =>
            {
                var packBehavior = references.GetCachedComponent<PackBehavior>();
                return new TransformTarget(packBehavior.GetPack().GetAlpha().gameObject.transform);
            });
        }

        // The Created method is called when the sensor is created
        // This can be used to gather references to objects in the scene
        public override void Created()
        {
        }

        // This method is equal to the Update method of a local sensor.
        // It can be used to cache data, like gathering a list of all pears in the scene.
        public override void Update()
        {
        }
    }
}