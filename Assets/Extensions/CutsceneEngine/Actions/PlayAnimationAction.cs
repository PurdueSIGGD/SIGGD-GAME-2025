namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Plays an animation on an actor. This is a one-shot action that triggers when the clip starts.
     * The animation will play for its full duration regardless of the clip length.
     * </summary>
     */
    [System.Serializable]
    public class PlayAnimationAction : CutsceneActionBase
    {
        public string AnimationId;

        public override void OnEnter(ICutsceneActor actor, CutsceneContext context)
        {
            if (context?.Animation != null && !string.IsNullOrEmpty(AnimationId))
            {
                context.Animation.PlayAnimation(actor, AnimationId);
            }
        }
    }
}

