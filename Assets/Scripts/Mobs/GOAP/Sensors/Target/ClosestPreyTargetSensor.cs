using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using SIGGD.Mobs;
using System.Collections.Generic;
using UnityEngine;

namespace SIGGD.Goap.Sensors
{
    public class ClosestPreyTargetSensor : LocalTargetSensorBase
    {

        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var perceptionManager = references.GetCachedComponent<PerceptionManager>();
            var AgentHuntBehaviour = references.GetCachedComponent<AgentHuntBehaviour>();
            if (perceptionManager == null || AgentHuntBehaviour == null) return null;

            if (AgentHuntBehaviour.currentTargetOfHunt != null && AgentHuntBehaviour.currentTargetOfHunt.activeInHierarchy)
                return new TransformTarget(AgentHuntBehaviour.currentTargetOfHunt.transform);
            var closestPrey = Closest(perceptionManager.preyTargets, agent.Transform.position);
            if (closestPrey == null)
                return null;

            AgentHuntBehaviour.SetHuntTarget(closestPrey.gameObject);
            if (existingTarget is TransformTarget transformTarget)
                return transformTarget.SetTransform(closestPrey.transform);
            return new TransformTarget(closestPrey.transform);
        }
        private GameObject Closest(List<GameObject> list, Vector3 position)
        {
            GameObject closest = null;
            var closestDistance = float.MaxValue;

            foreach (var item in list)
            {
                if (item == null) continue;
                var distance = Vector3.Distance(item.transform.position, position);

                if (!(distance < closestDistance))
                    continue;

                closest = item;
                closestDistance = distance;
            }

            return closest;
        }
        public override void Update()
        {
        }
    }
}