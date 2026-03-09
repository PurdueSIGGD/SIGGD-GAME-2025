namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The ICutsceneAnimationProvider interface defines a contract for providing animation actions for cutscenes.
     * Implementing this interface allows an object to specify how it should animate during cutscenes, such as playing walk, run, or idle animations.
     * This interface can be used by the cutscene system to invoke the appropriate animation actions on objects that implement it, enabling dynamic and context-specific animations during cutscenes.
     * </summary>
     */
    public interface ICutsceneAnimationProvider
    {
        /**
         * <summary>
         * Plays the walk animation for the object. This method should be implemented to trigger the appropriate walk animation on the object when called by the cutscene system.
         * The cutscene system will invoke this method at the appropriate time during a cutscene to ensure that the object animates correctly based on the cutscene's requirements.
         * </summary>
         */
        void PlayWalk();
        
        /**
         * <summary>
         * Plays the run animation for the object. This method should be implemented to trigger the appropriate run animation on the object when called by the cutscene system.
         * The cutscene system will invoke this method at the appropriate time during a cutscene to ensure that the object animates correctly based on the cutscene's requirements.
         * </summary>
         */
        void PlayRun();
        
        /**
         * <summary>
         * Plays the idle animation for the object. This method should be implemented to trigger the appropriate idle animation on the object when called by the cutscene system.
         * The cutscene system will invoke this method at the appropriate time during a cutscene to ensure that the object animates correctly based on the cutscene's requirements.
         * </summary>
         */
        void PlayIdle();
    }
}