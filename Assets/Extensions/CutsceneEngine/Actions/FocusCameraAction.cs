using System;
using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Focuses the camera on a target. This is a one-shot action that triggers when the clip starts.
     * The camera will remain focused until another camera action changes it.
     * </summary>
     */
    [Serializable]
    public class FocusCameraAction : CutsceneActionBase
    {
        [Tooltip("If true, focuses on the actor executing this action. If false, uses Explicit Target.")]
        public bool FocusOnSelf = true;
        
        [Tooltip("Target to focus on (used when Focus On Self is false)")]
        public GameObject ExplicitTarget;
        
        public override void OnEnter(ICutsceneActor actor, CutsceneContext context)
        {
            if (context?.Camera == null)
            {
                Debug.LogWarning("FocusCameraAction: No camera system available in context");
                return;
            }
            
            Transform target = FocusOnSelf ? actor.GetTransform() : ExplicitTarget?.transform;
            
            if (target == null)
            {
                Debug.LogWarning("FocusCameraAction: No valid target to focus on");
                return;
            }
            
            context.Camera.FocusOn(target);
        }
    }
}

