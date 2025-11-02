using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Interfaces;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Goap.Sensors
{
    public class SafetyTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {

        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var random = this.LocateRandomPosition(agent);
            // if the position target exists, update it with a new random position
            if (existingTarget is PositionTarget positionTarget)
            {
                return positionTarget.SetPosition(random);
            }
            // if the position target doesn't exist, create a new random target
            return new PositionTarget(random);
        }

        /// <summary>
        /// helper function to generate a random location in-world
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        private Vector3 LocateRandomPosition(IActionReceiver agent)
        {
            var random = Random.insideUnitSphere * 10f;
            random += agent.Transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(random, out hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            // Couldn't find a position on the navmesh, so just don't move
            return agent.Transform.position;
        }
        public override void Update()
        {

        }
    }
}
