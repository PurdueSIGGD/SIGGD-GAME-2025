using UnityEngine;
using UnityEngine.Playables;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutscenePlayableBehaviour class is a custom PlayableBehaviour used in Unity's Playable system to execute cutscene actions.
     * It properly supports Timeline scrubbing, clip duration, and continuous actions by calling OnEnter/OnUpdate/OnExit
     * on the action based on the clip's playback state.
     * </summary>
     */
    public class CutscenePlayableBehaviour : PlayableBehaviour
    {
        public CutsceneActionReference actionReference;
        public ICutsceneActor explicitActor;

        private bool firstFrame = true;
        private float lastTime;
        private ICutsceneActor cachedActor; // Cache actor for OnExit

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            // Reset state when clip starts
            firstFrame = true;
            lastTime = 0f;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var context = GetContext(playable);
            var actor = explicitActor ?? playerData as ICutsceneActor;
            
            // In edit mode, context will be null - that's okay for basic scrubbing
            // Only require actor and action
            if (actor == null || actionReference?.Action == null)
            {
                return;
            }

            // Cache actor for OnExit
            cachedActor = actor;

            // Call OnEnter on first frame
            if (firstFrame)
            {
                actionReference.Action.OnEnter(actor, context); // context can be null in edit mode
                firstFrame = false;
            }

            // Calculate normalized time (0.0 to 1.0 through the clip)
            double clipDuration = playable.GetDuration();
            double clipTime = playable.GetTime();
            float normalizedTime = clipDuration > 0 ? (float)(clipTime / clipDuration) : 0f;
            
            // Calculate delta time since last frame
            float currentTime = (float)clipTime;
            float deltaTime = currentTime - lastTime;
            lastTime = currentTime;

            // Call update every frame for continuous actions and scrubbing support
            actionReference.Action.OnUpdate(actor, context, normalizedTime, deltaTime);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // Called when clip stops playing (end of clip or Timeline stopped)
            if (!firstFrame) // Only call OnExit if OnEnter was called
            {
                var context = GetContext(playable);
                // Use cached actor from ProcessFrame
                if (context != null && cachedActor != null && actionReference?.Action != null)
                {
                    actionReference.Action.OnExit(cachedActor, context);
                }
                
                firstFrame = true;
                lastTime = 0f;
                cachedActor = null;
            }
        }

        private CutsceneContext GetContext(Playable playable)
        {
            var resolver = playable.GetGraph().GetResolver();
            if (resolver is not PlayableDirector director) return null;

            var wrapper = director.GetComponent<CutsceneDirector>();
            return wrapper?.Context;
        }
    }
}

