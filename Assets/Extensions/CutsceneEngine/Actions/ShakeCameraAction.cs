using System;
using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Applies a shake effect to the camera. This is a one-shot action that triggers when the clip starts.
     * The shake will persist for the specified duration regardless of the clip length.
     * </summary>
     */
    [Serializable]
    public class ShakeCameraAction : CutsceneActionBase
    {
        [Tooltip("Intensity of the camera shake (higher values = more shake)")]
        [Range(0f, 10f)]
        public float Intensity = 1f;
        
        [Tooltip("Duration of the camera shake in seconds")]
        public float Duration = 0.5f;
        
        public override void OnEnter(ICutsceneActor actor, CutsceneContext context)
        {
            if (context?.Camera == null)
            {
                Debug.LogWarning("ShakeCameraAction: No camera system available in context");
                return;
            }
            
            context.Camera.Shake(Intensity, Duration);
        }
    }
}

