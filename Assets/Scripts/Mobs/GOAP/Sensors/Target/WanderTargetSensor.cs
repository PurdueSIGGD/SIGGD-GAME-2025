using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Interfaces;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.AI;
using SIGGD.Mobs;
using CrashKonijn.Goap.Core;

namespace SIGGD.Goap.Sensors
{
    public class WanderTargetSensor : LocalTargetSensorBase
    {
        private Smell smell;
       // private NavMeshQueryFilter filter;
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            //this.filter = references.GetCachedComponent<AgentData>().filter;
            smell = references.GetCachedComponent<Smell>();
            var random = this.LocateRandomPosition(agent, smell);
            var navPos = Pathfinding.ShiftTargetToNavMesh(random, 10f);
            NavMeshPath path = new NavMeshPath();
            bool validPath = NavMesh.CalculatePath(agent.Transform.position, navPos, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
            if (!validPath) return null;
            if (existingTarget is PositionTarget positionTarget)
            {
                return positionTarget.SetPosition(navPos);
            }
            return new PositionTarget(navPos);
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
            Debug.Log(smellPos);
            if (smellPos != Vector3.zero)
            {
                Vector3 toSmell = (smell.GetSmellPos() - agent.Transform.position);
                dir = Vector3.Slerp(toRandom, toSmell, biasStrength).normalized;
            } else
            {
                dir = toRandom;
            }
            return agent.Transform.position + dir * 10f;
        }
        public override void Update()
        {

        }

    }
}
