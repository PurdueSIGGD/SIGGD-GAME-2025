using System;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Base class for cutscene actions that provides default implementations of ICutsceneAction.
     * Inherit from this class to:
     * - Only override the methods you need
     * - Get empty implementations for methods you don't use
     * 
     * Common patterns:
     * - One-shot actions: Override only OnEnter
     * - Continuous actions: Override OnEnter (init), OnUpdate (interpolate), OnExit (cleanup)
     * - Simple interpolations: Override OnUpdate only
     * </summary>
     */
    [Serializable]
    public abstract class CutsceneActionBase : ICutsceneAction
    {
        /**
         * <summary>
         * Called once when clip starts. Override for initialization or one-shot actions.
         * Default: Does nothing.
         * </summary>
         */
        public virtual void OnEnter(ICutsceneActor actor, CutsceneContext context)
        {
            // Default: No-op
        }

        /**
         * <summary>
         * Called every frame while clip is active. Override for continuous actions and scrubbing support.
         * Default: Does nothing.
         * </summary>
         * <param name="actor">Target actor</param>
         * <param name="context">Cutscene context</param>
         * <param name="normalizedTime">0.0 at start, 1.0 at end</param>
         * <param name="deltaTime">Time since last frame</param>
         */
        public virtual void OnUpdate(ICutsceneActor actor, CutsceneContext context, float normalizedTime, float deltaTime)
        {
            // Default: No-op
        }

        /**
         * <summary>
         * Called once when clip ends. Override for cleanup.
         * Default: Does nothing.
         * </summary>
         */
        public virtual void OnExit(ICutsceneActor actor, CutsceneContext context)
        {
            // Default: No-op
        }
    }
}

