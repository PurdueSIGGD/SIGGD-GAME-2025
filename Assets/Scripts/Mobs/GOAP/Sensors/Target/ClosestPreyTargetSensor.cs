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

        private PreyBehaviour[] prey;
        
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var AgentHuntBehaviour = references.GetCachedComponent<AgentHuntBehaviour>();
            Debug.Log($"current target is null {AgentHuntBehaviour.currentTargetOfHunt == null}");
            if (AgentHuntBehaviour.currentTargetOfHunt != null && AgentHuntBehaviour.currentTargetOfHunt.activeInHierarchy)
                return new TransformTarget(AgentHuntBehaviour.currentTargetOfHunt.transform);
            var closestPrey = Closest(prey, agent.Transform.position);
            if (closestPrey == null)
                return null;

            AgentHuntBehaviour.SetHuntTarget(closestPrey.gameObject);
            Debug.Log(closestPrey.gameObject);
            if (existingTarget is TransformTarget transformTarget)
                return transformTarget.SetTransform(closestPrey.transform);
            return new TransformTarget(closestPrey.transform);
        }
        private T Closest<T>(IEnumerable<T> list, Vector3 position)
            where T : MonoBehaviour
        {
            T closest = null;
            var closestDistance = float.MaxValue;

            foreach (var item in list)
            {
                var distance = Vector3.Distance(item.gameObject.transform.position, position);

                if (!(distance < closestDistance))
                    continue;

                closest = item;
                closestDistance = distance;
            }

            return closest;
        }
        public override void Update()
        {
            this.prey = Object.FindObjectsByType<PreyBehaviour>(FindObjectsSortMode.None);
        }
    }
}
    