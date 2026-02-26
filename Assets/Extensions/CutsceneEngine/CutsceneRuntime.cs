namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutsceneRuntime class manages the state of cutscenes during runtime, providing a simple interface to check if a cutscene is currently active.
     * It allows for other parts of the game to query whether a cutscene is playing, enabling them to adjust behavior accordingly (e.g., pausing player input).
     * The class provides methods to begin and end a cutscene, which can be called by the cutscene system when starting and finishing a cutscene sequence.
     * </summary>
     */
    public static class CutsceneRuntime
    {
        /**
         * <summary>
         * Indicates whether a cutscene is currently active. This property can be used by other parts of the game to determine if a cutscene is playing and adjust behavior accordingly (e.g., pausing player input).
         * The value of this property is managed by the BeginCutscene and EndCutscene methods, which should be called by the cutscene system when starting and finishing a cutscene sequence.
         * </summary>
         */
        public static bool IsCutsceneActive { get; private set; }

        /**
         * <summary>
         * Begins a cutscene by setting the IsCutsceneActive property to true. This method should be called by the cutscene system when starting a cutscene sequence to indicate that a cutscene is now active.
         * Other parts of the game can check the IsCutsceneActive property to adjust behavior accordingly (e.g., pausing player input) while the cutscene is playing.
         * </summary>
         */
        public static void BeginCutscene()
        {
            IsCutsceneActive = true;
        }

        /**
         * <summary>
         * Ends a cutscene by setting the IsCutsceneActive property to false. This method should be called by the cutscene system when finishing a cutscene sequence to indicate that the cutscene is no longer active.
         * Other parts of the game can check the IsCutsceneActive property to adjust behavior accordingly (e.g., resuming player input) once the cutscene has finished.
         * </summary>
         */
        public static void EndCutscene()
        {
            IsCutsceneActive = false;
        }
    }

}