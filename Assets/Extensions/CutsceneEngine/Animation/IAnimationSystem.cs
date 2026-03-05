namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The IAnimationSystem interface defines a contract for an animation system that can be used within the cutscene engine.
     * Implementing this interface allows for the management and execution of animations on cutscene actors, such as playing walk, idle, or custom animations based on the provided animation ID.
     * This interface can be used by the cutscene system to invoke the appropriate animation actions on cutscene actors during cutscenes, enabling dynamic and context-specific animations.
     * </summary>
     */
    public interface IAnimationSystem
    {
        /**
         * <summary>
         * Plays a custom animation for the specified cutscene actor based on the provided animation ID. This method should be implemented to trigger the appropriate animation on the actor when called by the cutscene system.
         * The cutscene system will invoke this method at the appropriate time during a cutscene, passing in the relevant animation ID to ensure that the actor animates correctly based on the cutscene's requirements.
         * </summary>
         * <param name="actor">The cutscene actor on which the custom animation should be played. This parameter provides access to the actor's properties and methods, allowing for interaction with the actor during the execution of the animation.</param>
         * <param name="id">The identifier for the custom animation to be played. This parameter allows for specifying different animations based on the context of the cutscene, enabling dynamic and context-specific animations.</param>
         */
        void PlayAnimation(ICutsceneActor actor, string id);
    }
}