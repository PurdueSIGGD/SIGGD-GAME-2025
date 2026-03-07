using UnityEngine;
using UnityEngine.Playables;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutsceneActionPlayableBehaviour class is a custom PlayableBehaviour used in Unity's
     * Playables system to execute cutscene actions defined by the CutsceneActionDefinition. It processes each frame of the playable and triggers the specified cutscene action on the target MonoBehaviour when the conditions are met.
     * This class allows for the integration of cutscene actions into Unity's timeline, enabling dynamic
     * execution of cutscene actions based on the timeline's progression.
     * </summary>
     */
    public class CutsceneActionPlayableBehaviour : PlayableBehaviour
    {
        public MonoBehaviour target;
        public CutsceneActionDefinition action;

        private bool triggered = false;

        /**
         * <summary>
         * Processes each frame of the playable and triggers the specified cutscene action on the target MonoBehaviour when the conditions are met. This method checks if the action has already been triggered, if the target and action are valid, and if the target implements the ICutsceneOverridable interface. If all conditions are satisfied, it retrieves the cutscene adapter from the target and invokes the specified action with its parameters.
         * </summary>
         * <param name="playable">The playable being processed.</param>
         * <param name="info">Information about the current frame being processed.</param>
         * <param name="playerData">Additional data associated with the player, which can be used for context during processing.</param>
         */
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (triggered || target == null || action == null) return;

            var overridable = target as ICutsceneOverridable;
            if (overridable == null) return;

            var adapter = overridable.GetCutsceneAdapter();
            var args = action.parameters.ConvertAll(p => p.GetValue()).ToArray();

            adapter.Invoke(action.name, args);

            triggered = true;
        }
    }

}