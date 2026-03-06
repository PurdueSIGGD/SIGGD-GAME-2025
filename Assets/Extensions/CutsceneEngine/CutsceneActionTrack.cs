using UnityEngine;
using UnityEngine.Timeline;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutsceneActionTrack class represents a track in Unity's Timeline that can contain CutsceneActionClips.
     * This class allows for organizing and managing cutscene actions within the Timeline, enabling designers to create
     * complex cutscenes with custom actions. The track binds to MonoBehaviour components that implement ICutsceneActor.
     * </summary>
     */
    [TrackClipType(typeof(CutsceneActionClip))]
    [TrackBindingType(typeof(MonoBehaviour))]
    [TrackColor(0.4f, 0.6f, 1f)] // Light blue color for easy identification
    public class CutsceneActionTrack : TrackAsset { }
}