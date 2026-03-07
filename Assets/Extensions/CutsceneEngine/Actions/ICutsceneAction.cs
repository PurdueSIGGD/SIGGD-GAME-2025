namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The ICutsceneAction interface defines the contract for cutscene actions.
     * Actions can execute in two modes:
     * 1. One-shot execution (OnEnter) - Execute once when clip starts
     * 2. Continuous execution (OnUpdate) - Execute every frame, with normalized time for scrubbing support
     * 
     * Implementing both methods allows actions to:
     * - Initialize state (OnEnter)
     * - Update state based on Timeline playback time (OnUpdate)
     * - Support Timeline scrubbing (dragging the playhead)
     * - Respond to clip duration changes
     * </summary>
     */
    public interface ICutsceneAction
    {
        /**
         * <summary>
         * Called once when the clip begins playback. Use this for one-shot actions
         * or to initialize state for continuous actions.
         * </summary>
         * <param name="actor">The actor this action targets</param>
         * <param name="context">The cutscene context with system references</param>
         */
        void OnEnter(ICutsceneActor actor, CutsceneContext context);
        
        /**
         * <summary>
         * Called every frame while the clip is active. Use this for continuous actions
         * that need to respond to Timeline scrubbing and duration.
         * 
         * normalizedTime = 0.0 at clip start, 1.0 at clip end
         * This allows actions to properly interpolate and support scrubbing.
         * </summary>
         * <param name="actor">The actor this action targets</param>
         * <param name="context">The cutscene context with system references</param>
         * <param name="normalizedTime">Time through clip (0.0 to 1.0)</param>
         * <param name="deltaTime">Time since last frame (for frame-independent updates)</param>
         */
        void OnUpdate(ICutsceneActor actor, CutsceneContext context, float normalizedTime, float deltaTime);
        
        /**
         * <summary>
         * Called once when the clip ends. Use this to clean up state or finalize the action.
         * </summary>
         * <param name="actor">The actor this action targets</param>
         * <param name="context">The cutscene context with system references</param>
         */
        void OnExit(ICutsceneActor actor, CutsceneContext context);
    }
}