using System;
using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap;
using SIGGD.Mobs.PackScripts;
using UnityEngine;

namespace SIGGD.Goap.Sensors
{
    public class PackMultiSensor : MultiSensorBase
    {
        // You must use the constructor to register all the sensors
        // This can also be called outside of the gameplay loop to validate the configuration
        public PackMultiSensor()
        {
            this.AddLocalWorldSensor<CloseToAlpha>((agent, references) =>
            {
                var packBehavior = references.GetCachedComponent<PackBehavior>();
                return packBehavior.GetCloseToAlphaKey();
            });
            this.AddLocalWorldSensor<IsAlpha>((agent, references) =>
            {
                var packBehavior = references.GetCachedComponent<PackBehavior>();
                return packBehavior.GetIsAlphaKey();
            });
            this.AddLocalWorldSensor<LargePack>((agent, references) =>
            {
                var packBehavior = references.GetCachedComponent<PackBehavior>();
                if (packBehavior.GetPack() == null)
                {
                    return 0;
                }
                return packBehavior.GetPack().GetSize() == packBehavior.Data.MaxPackSize ? 1 : 0;
            });
            this.AddLocalWorldSensor<InPack>((agent, references) =>
            {
                var packBehavior = references.GetCachedComponent<PackBehavior>();
                return packBehavior.GetPack() != null;
            });
            this.AddLocalTargetSensor<PackAlphaTarget>((agent, references, target) =>
            {
                var packBehavior = references.GetCachedComponent<PackBehavior>();
                if (packBehavior.GetPack() == null || packBehavior.GetPack().GetAlpha() == null)
                    return null;
                PackBehavior alpha = packBehavior.GetPack().GetAlpha();
                return new TransformTarget(alpha.gameObject.transform);
            });
            this.AddLocalTargetSensor<PackClosestTarget>((agent, references, target) =>
            {
                var packBehavior = references.GetCachedComponent<PackBehavior>();
                if (packBehavior.IsHappyWithPack())
                    return null;
                PackBehavior neighbor = packBehavior.FindNearbyNeighbor(excludePack: true);
                if (neighbor == null || !PackManager.CanJoin(packBehavior, neighbor, excludePack: true))
                    return null;
                return new TransformTarget(neighbor.gameObject.transform);
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