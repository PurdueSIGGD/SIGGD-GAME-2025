using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Moves an actor from their current position to a target position over the clip duration.
     * Supports Timeline scrubbing - dragging the playhead will update actor position in real-time.
     * </summary>
     */
    [System.Serializable]
    public class MoveActorAction : CutsceneActionBase
    {
        public Vector3 StartPosition = Vector3.zero;
        public bool UseStartPositionFromActor = true; // If true, will ignore StartPosition and use actor's current position at OnEnter
        public Vector3 Target;

        [System.NonSerialized]
        private Vector3 startPosition;
        [System.NonSerialized]
        private bool hasInitialized;

        public override void OnEnter(ICutsceneActor actor, CutsceneContext context)
        {
            // Cache start position for interpolation
            startPosition = UseStartPositionFromActor ? actor.GetTransform().position : StartPosition;

            if (UseStartPositionFromActor)
            {
                // If we're using the actor's current position, we need to set the actor to that position at the start of the clip
                actor.GetTransform().position = startPosition;
            }
            
            hasInitialized = true;
        }

        public override void OnUpdate(ICutsceneActor actor, CutsceneContext context, float normalizedTime, float deltaTime)
        {
            // Ensure we have a start position (in case OnEnter wasn't called due to scrubbing)
            if (!hasInitialized)
            {
                startPosition = UseStartPositionFromActor ? actor.GetTransform().position : StartPosition;
                
                if (UseStartPositionFromActor)
                {
                    // If we're using the actor's current position, we need to set the actor to that position at the start of the clip
                    actor.GetTransform().position = startPosition;
                }
                
                hasInitialized = true;
            }
            
            // If normalizedTime is 1 or greater, ensure we set the final position and exit early
            if (normalizedTime >= 1f)
            {
                actor.GetTransform().position = Target;
                return;
            }
            
            // Lerp position based on normalized time (supports scrubbing)
            Vector3 targetPos = Vector3.Lerp(startPosition, Target, normalizedTime);
            actor.GetTransform().position = targetPos;
        }

        public override void OnExit(ICutsceneActor actor, CutsceneContext context)
        {
            // Ensure actor reaches exact target position
            actor.GetTransform().position = Target;
            
            // Reset for next time
            hasInitialized = false;
        }
    }
}

