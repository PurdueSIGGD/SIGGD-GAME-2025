using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

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
        
        private Vector3 LocateRandomPosition(IActionReceiver agent)
        {
            var random = Random.insideUnitCircle * 10f;
            var position = agent.Transform.position + new Vector3(random.x, 0, random.y);

            return position;
      
        }
        public override void Update()
        {

        }
    }


}
