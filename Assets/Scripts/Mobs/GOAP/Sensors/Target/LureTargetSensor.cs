using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Goap.Sensors
{
    public class LureTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {

            if (agent == null)
            {
                return null;
            }
            Lure closestLure = ClosestLure(agent.Transform.position);
            if (closestLure == null )
                return null;
            return new PositionTarget(closestLure.transform.position);
        }
        public override void Update()
        {
        }


        private Lure ClosestLure(Vector3 position)
        {
            Lure closest = default;
            var closestDistance = float.MaxValue;
            foreach (var lure in LureManager.ActiveLures)
            {
                if (lure == null) continue;
                var distance = Vector3.Distance(lure.transform.position, position);
                if ((distance > lure.radius) ||
                    (!(distance < closestDistance)))
                {
                    continue;
                }
                closest = lure;
                closestDistance = distance;
            }
            return closest;
        }
    }
}