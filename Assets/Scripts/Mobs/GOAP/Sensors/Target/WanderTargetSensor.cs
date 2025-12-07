using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Interfaces;
using SIGGD.Mobs;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.AI;
using Utility;

namespace SIGGD.Goap.Sensors
{
    public class WanderTargetSensor : LocalTargetSensorBase
    {
        private Smell smell;
        private Boundary boundary;
        public override void Created()
        {

        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            //var filter = references.GetCachedComponent<AgentData>().filter;
            this.boundary = references.GetCachedComponent<AgentData>().boundary;
            smell = references.GetCachedComponent<Smell>();
            var random = LocateRandomPositionWithinBoundary(agent, smell);
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

        public override void Update()
        {
        }

        /// <summary>
        /// helper function to generate a random location in-world
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        ///

        private Vector3 LocateRandomPositionWithinBoundary(IActionReceiver agent, Smell smell)
        {
            Vector3 resultPos = agent.Transform.position;
            for (int i = 0; i < 10; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * boundary.MaxDist;
                Vector2 query = boundary.Centroid + randomOffset;
                if (boundary.IsInBoundary(query))
                {
                    resultPos = new Vector3(query.x, agent.Transform.position.y, query.y);
                    break;
                }
            }
            Vector3 toRandom = (resultPos - agent.Transform.position).normalized;

            float biasStrength = 0.7f;
            Vector3 smellPos = smell.GetSmellPos();
            Vector3 dir;
            if (smellPos != Vector3.zero)
            {
                Vector3 toSmell = (smell.GetSmellPos() - agent.Transform.position);
                dir = Vector3.Slerp(toRandom, toSmell, biasStrength).normalized;
            }
            else
            {
                dir = toRandom;
            }
            Vector3 target = agent.Transform.position + dir * 10f;
            Vector2 queryTarget = new Vector2(target.x, target.y);
            if (!boundary.IsInBoundary(queryTarget))
                target = resultPos;
            return target;
        }
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
            }
            else
            {
                dir = toRandom;
            }
            return agent.Transform.position + dir * 10f;
        }
    }
}