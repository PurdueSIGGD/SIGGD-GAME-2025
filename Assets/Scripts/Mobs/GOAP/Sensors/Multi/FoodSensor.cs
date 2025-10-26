using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using SIGGD.Mobs;

namespace SIGGD.Goap.Sensors
{
    public class FoodSensor : MultiSensorBase
    {
        private FoodBehaviour[] food;

        public FoodSensor() {
            this.AddLocalWorldSensor<FoodCount>((agent, references) =>
            {
                var data = references.GetCachedComponent<HungerBehaviour>();
                return food.Length;
            });
            this.AddLocalWorldSensor<Hunger>((agent, references) =>
            {
                var data = references.GetCachedComponent<HungerBehaviour>();
                return (int)references.GetCachedComponent<HungerBehaviour>().hunger;
            });
        this.AddLocalTargetSensor<ClosestFood>((agent, references, target) => {
                var closestFood = Closest(food, agent.Transform.position);
                if (closestFood == null)
                    return null;
                if (target is TransformTarget transformTarget)
                    return transformTarget.SetTransform(closestFood.transform);
                return new TransformTarget(closestFood.transform);
            });
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
        public override void Created()
        {
        }

        public override void Update()
        {
            this.food = Object.FindObjectsByType<FoodBehaviour>(FindObjectsSortMode.None);

        }
    }
}
    