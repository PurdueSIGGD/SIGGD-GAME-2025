using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Goap.Sensors
{
    public class WanderTargetSensor : LocalTargetSensorBase
    {
        private Smell smell;
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            smell = references.GetCachedComponent<Smell>();
            var random = this.LocateRandomPosition(agent, smell);
            var randMesh = Pathfinding.ShiftTargetToNavMesh(random, 10f);
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
        private Vector3 LocateRandomPosition(IActionReceiver agent, Smell smell)
        {
            var randomInCircle = Random.insideUnitCircle * 50f;
            var random3D = new Vector3(randomInCircle.x, 0f, randomInCircle.y);
            Vector3 randomPos = agent.Transform.position + random3D;
            
            Vector3 toRandom = (randomPos - agent.Transform.position).normalized;

            float biasStrength = 0.7f;
            Vector3 smellPos = smell.GetSmellPos();
            Vector3 dir;
            if (smellPos != Vector3.zero)
            {
                Vector3 toSmell = (smell.GetSmellPos() - agent.Transform.position);
                dir = Vector3.Slerp(toRandom, toSmell, biasStrength).normalized;
            } else
            {
                dir = toRandom;
            }
            return agent.Transform.position + dir * 50f;
        }
        public override void Update()
        {

        }
    }
}
