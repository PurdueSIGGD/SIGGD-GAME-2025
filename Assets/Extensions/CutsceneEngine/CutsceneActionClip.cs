using UnityEngine;
using UnityEngine.Playables;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutsceneActionClip class is a custom PlayableAsset used in Unity's Playable system to represent a clip that executes a cutscene action.
     * It is used in conjunction with the CutscenePlayableBehaviour to execute specified cutscene actions on target actors during a cutscene sequence.
     * </summary>
     */
    public class CutsceneActionClip : PlayableAsset
    {
        public CutsceneActionReference action;
        public MonoBehaviour explicitTarget;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CutscenePlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            behaviour.actionReference = action;

            if (explicitTarget is ICutsceneActor actor)
                behaviour.explicitActor = actor;

            return playable;
        }
    }


}