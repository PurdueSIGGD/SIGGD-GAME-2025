using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The ICameraSystem interface defines the contract for camera control systems within the cutscene engine.
     * It provides methods for focusing on a target, moving to a specific position, and shaking the camera.
     * Implementations of this interface can be used to create different camera behaviors during cutscenes, allowing for dynamic and cinematic camera movements.
     * This interface is designed to be flexible and can be implemented in various ways depending on the specific requirements of the cutscene and the desired camera effects.
     * </summary>
     */
    public interface ICameraSystem
    {
        /**
         * <summary>
         * Focuses the camera on a specified target. This method should be implemented to adjust the camera's position and orientation to focus on the given target transform.
         * The cutscene system will invoke this method at the appropriate time during a cutscene, passing in the target transform to ensure that the camera focuses correctly based on the cutscene's requirements.
         * </summary>
         * <param name="target">The transform of the target that the camera should focus on. This parameter provides access to the target's position and orientation, allowing for dynamic camera adjustments based on the target's properties.</param>
         */
        void FocusOn(Transform target);
        
        /**
         * <summary>
         * Moves the camera to a specified position over a given duration. This method should be implemented to smoothly transition the camera's position to the target position over the specified time.
         * The cutscene system will invoke this method at the appropriate time during a cutscene, passing in the target position and duration to ensure that the camera moves correctly based on the cutscene's requirements.
         * </summary>
         * <param name="position">The target position that the camera should move to. This parameter provides the coordinates for the desired camera location, allowing for dynamic camera movements based on the cutscene's context.</param>
         * <param name="duration">The duration over which the camera should move to the target position. This parameter specifies how long the camera transition should take, enabling smooth and cinematic camera movements during cutscenes.</param>
         */
        void MoveTo(Vector3 position, float duration);
        
        /**
         * <summary>
         * Shakes the camera with a specified intensity and duration. This method should be implemented to create a shaking effect on the camera, simulating impacts or other dynamic events during cutscenes.
         * The cutscene system will invoke this method at the appropriate time during a cutscene, passing in the intensity and duration to ensure that the camera shake effect is applied correctly based on the cutscene's requirements.
         * </summary>
         * <param name="intensity">The intensity of the camera shake. This parameter specifies how strong the shaking effect should be, allowing for varying levels of impact based on the context of the cutscene.</param>
         * <param name="duration">The duration of the camera shake. This parameter specifies how long the shaking effect should last, enabling dynamic and context-specific camera effects during cutscenes.</param>
         */
        void Shake(float intensity, float duration);
    }
}