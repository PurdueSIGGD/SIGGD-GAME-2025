using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The ICutsceneActor interface represents an actor that can be used in cutscenes. It extends the ICutsceneOverridable interface, allowing for cutscene-specific overrides of actor properties and behaviors.
     * Implementing this interface allows a GameObject to be recognized as a cutscene actor and enables it to participate in cutscene actions and events.
     * </summary>
     */
    public interface ICutsceneActor : ICutsceneOverridable
    {
        /**
         * <summary>
         * Retrieves the Transform component of the cutscene actor. This method is essential for positioning, rotating, and scaling the actor within the cutscene.
         * </summary>
         * <returns>The Transform component of the cutscene actor.</returns>
         */ 
        Transform GetTransform();
    }
}