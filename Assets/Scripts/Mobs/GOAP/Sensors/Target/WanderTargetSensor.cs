using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Goap.Sensors
{
    public class WanderTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {

        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var random = this.LocateRandomPosition(agent);
            var randMesh = Pathfinding.ShiftTargetToNavMesh(random);
            if (existingTarget is PositionTarget positionTarget)
            {
                return positionTarget.SetPosition(randMesh);
            }
            return new PositionTarget(randMesh);
        }
        
        /// <summary>
        /// helper function to generate a random location in-world
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        private Vector3 LocateRandomPosition(IActionReceiver agent)
        {
            var randomInCircle = Random.insideUnitCircle * 50f;
            var random3D = new Vector3(randomInCircle.x, UnityEngine.Random.Range(-10f, 10f), randomInCircle.y);
            random3D += agent.Transform.position;
            return random3D;
        }
        public override void Update()
        {

        }
    }
}
