using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutsceneDirector class is responsible for managing the playback of cutscenes in the game.
     * It utilizes Unity's PlayableDirector to control the timeline of the cutscene and interacts with ICutsceneActor instances to execute actions during the cutscene.
     * The CutsceneDirector also manages a CinemachineCamera for cinematic camera control during cutscenes.
     * </summary>
     */
    public class CutsceneDirector : MonoBehaviour
    {
        public PlayableDirector director;
        public CinemachineCamera cinematicCamera;

        public CutsceneContext Context { get; private set; }
        private List<ICutsceneActor> activeActors = new();

        /**
         * <summary>
         * Plays the cutscene by building the CutsceneContext, registering active actors, and starting the PlayableDirector.
         * This method initializes the context for the cutscene, identifies all actors involved in the cutscene, and begins playback of the timeline.
         * It also calls OnCutsceneEnter on each active actor to allow them to perform any necessary setup before the cutscene actions are executed.
         * </summary>
         */
        public void Play()
        {
            Context = CutsceneContextBuilder.Build(this, cinematicCamera);

            RegisterActors();

            foreach (var actor in activeActors)
                actor.OnCutsceneEnter();

            CutsceneRuntime.BeginCutscene();

            director.Play();
        }

        /**
         * <summary>
         * Stops the cutscene by stopping the PlayableDirector, calling OnCutsceneExit on each active actor, and clearing the list of active actors.
         * This method ensures that all actors involved in the cutscene are properly notified of the cutscene's end and that any necessary cleanup is performed.
         * It also signals the CutsceneRuntime to end the cutscene, allowing for any global state or systems to be updated accordingly.
         * </summary>
         */
        public void Stop()
        {
            director.Stop();

            foreach (var actor in activeActors)
                actor.OnCutsceneExit();

            activeActors.Clear();

            CutsceneRuntime.EndCutscene();
        }

        /**
         * <summary>
         * Registers active actors by iterating through the outputs of the PlayableDirector's playable asset and checking for bindings that implement the ICutsceneActor interface.
         * This method populates the list of active actors that will be involved in the cutscene, allowing them to be notified when the cutscene starts and ends.
         * It ensures that all relevant actors are identified and registered for interaction during the cutscene playback.
         * </summary>
         */
        private void RegisterActors()
        {
            activeActors.Clear();

            foreach (var output in director.playableAsset.outputs)
            {
                var bound = director.GetGenericBinding(output.sourceObject);

                if (bound is ICutsceneActor actor)
                    activeActors.Add(actor);
            }
        }
    }
}