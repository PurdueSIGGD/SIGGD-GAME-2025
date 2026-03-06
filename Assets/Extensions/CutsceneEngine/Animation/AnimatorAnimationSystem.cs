using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The AnimatorAnimationSystem class implements the IAnimationSystem interface to provide animation functionality using Unity's Animator component.
     * It allows cutscene actors to play specific animations by referencing their Animator component and executing the desired animation state.
     * This class is designed to be used within the cutscene engine to facilitate the playback of animations on actors during cutscenes, enhancing the visual storytelling and immersion of the scene.
     * </summary>
     */
    public class AnimatorAnimationSystem : IAnimationSystem
    {
        public void PlayAnimation(ICutsceneActor actor, string id)
        {
            var animator = actor.GetTransform().GetComponent<Animator>();
            if (animator) animator.Play(id);
        }
    }
}