namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Marker interface for cutscene elements that can be overridden by the cutscene editor.
     * </summary>
     */
    public interface ICutsceneOverridable
    {
        /* <summary>
         * Retrieves the CutsceneActionAdapter associated with this object, which is used to invoke cutscene actions defined in the cutscene system. This method should return an instance of CutsceneActionAdapter that provides the necessary functionality to execute cutscene actions on this object.
         * The returned adapter will be used by the cutscene system to invoke actions defined in the cutscene timeline, allowing for dynamic execution of cutscene actions based on the timeline's progression.
         * </summary>
         * <returns>An instance of CutsceneActionAdapter associated with this object.</returns>
         */
        CutsceneActionAdapter GetCutsceneAdapter();

        /* <summary>
         * Method called when a cutscene starts. This method can be used to perform any necessary setup or initialization when a cutscene begins, such as preparing the object for cutscene actions or resetting its state.
         * The cutscene system will call this method at the appropriate time during the cutscene lifecycle, allowing for custom behavior to be executed when a cutscene starts.
         * </summary>
         */
        void OnCutsceneEnter();
        
        /* <summary>
         * Method called when a cutscene ends. This method can be used to perform any necessary cleanup or finalization when a cutscene concludes, such as resetting the object's state or performing any necessary actions after the cutscene has finished.
         * The cutscene system will call this method at the appropriate time during the cutscene lifecycle, allowing for custom behavior to be executed when a cutscene ends.
         * </summary>
         */
        void OnCutsceneExit();
    }
}