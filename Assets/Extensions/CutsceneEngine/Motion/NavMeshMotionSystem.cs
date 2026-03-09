using UnityEngine;
using UnityEngine.AI;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The NavMeshMotionSystem class implements the IMotionSystem interface to provide movement and rotation functionality for cutscene actors using Unity's NavMesh system.
     * The Move method uses a NavMeshAgent component to move the actor towards a specified target position over a given duration, while the Rotate method directly sets the actor's rotation to a specified Quaternion value.
     * This implementation allows for smooth and pathfinding-based movement of cutscene actors, as well as precise control over their orientation during cutscenes.
     * By utilizing Unity's NavMesh system, the NavMeshMotionSystem can handle complex navigation scenarios, such as avoiding obstacles and finding optimal paths to the target position.
     * </summary>
     */
    public class NavMeshMotionSystem : IMotionSystem
    {
        public void Move(ICutsceneActor actor, Vector3 target, float duration)
        {
            var agent = actor.GetTransform().GetComponent<NavMeshAgent>();
            if (agent == null) return;

            agent.SetDestination(target);
        }

        public void Rotate(ICutsceneActor actor, Quaternion rotation, float duration)
        {
            actor.GetTransform().rotation = rotation;
        }
    }
}