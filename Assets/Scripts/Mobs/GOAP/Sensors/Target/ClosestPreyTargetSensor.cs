using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using System.Collections.Generic;
using SIGGD.Goap.Behaviours;
using UnityEngine;

namespace SIGGD.Goap.Sensors
{
    public class ClosestPreyTargetSensor : LocalTargetSensorBase
    {

        private FoodBehaviour[] food;
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var closestFood = Closest(food, agent.Transform.position);
            if (closestFood == null)
                return null;

            // If the target is a transform target, set the target to the closest pear
            if (existingTarget is TransformTarget transformTarget)
                return transformTarget.SetTransform(closestFood.transform);

            return new TransformTarget(closestFood.transform);
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
            this.food = Object.FindObjectsByType<FoodBehaviour>(FindObjectsSortMode.None);

        }
    }
}
    