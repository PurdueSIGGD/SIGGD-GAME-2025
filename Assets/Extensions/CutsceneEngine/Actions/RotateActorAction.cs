using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Rotates an actor from their current rotation to a target rotation over the clip duration.
     * Supports Timeline scrubbing - dragging the playhead will update actor rotation in real-time.
     * </summary>
     */
    [System.Serializable]
    public class RotateActorAction : CutsceneActionBase
    {
        public Vector3 EulerRotation;

        [System.NonSerialized]
        private Quaternion startRotation;

        public override void OnEnter(ICutsceneActor actor, CutsceneContext context)
        {
            // Cache start rotation for interpolation
            startRotation = actor.GetTransform().rotation;
        }

        public override void OnUpdate(ICutsceneActor actor, CutsceneContext context, float normalizedTime, float deltaTime)
        {
            // If normalizedTime is 1 or greater, ensure we set the final rotation and exit early
            if (normalizedTime >= 1f)            
            {
                actor.GetTransform().rotation = Quaternion.Euler(EulerRotation);
                return;
            }
            
            // Slerp rotation based on normalized time (supports scrubbing)
            Quaternion targetRot = Quaternion.Euler(EulerRotation);
            actor.GetTransform().rotation = Quaternion.Slerp(startRotation, targetRot, normalizedTime);
        }

        public override void OnExit(ICutsceneActor actor, CutsceneContext context)
        {
            // Ensure actor reaches exact target rotation
            actor.GetTransform().rotation = Quaternion.Euler(EulerRotation);
        }
    }
}

