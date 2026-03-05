using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The IMotionSystem interface defines the contract for a motion system that can be used in cutscenes to move and rotate actors.
     * It provides methods for moving an actor to a target position over a specified duration and rotating an actor to a target rotation over a specified duration.
     * Implementing this interface allows for different motion systems to be created, enabling flexibility in how actors are animated during cutscenes.
     * The Move method is responsible for moving the actor to the target position, while the Rotate method is responsible for rotating the actor to the target rotation.
     * Both methods take into account the duration of the movement or rotation, allowing for smooth transitions and animations during cutscenes.
     * </summary>
     */
    public interface IMotionSystem
    {
        /**
         * <summary>
         * Moves the specified actor to the target position over the given duration. This method should be implemented to define how the actor's position is updated over time, allowing for smooth and controlled movement during cutscenes.
         * The cutscene system will call this method at the appropriate time during a cutscene, passing in the relevant actor, target position, and duration information to allow for dynamic execution of the movement based on the cutscene's requirements.
         * </summary>
         * <param name="actor">The cutscene actor that should be moved. This parameter provides access to the actor's properties and methods, allowing for interaction with the actor during the execution of the movement.</param>
         * <param name="target">The target position to which the actor should be moved. This parameter specifies the desired final position of the actor after the movement is completed.</param>
         * <param name="duration">The duration over which the movement should occur. This parameter specifies how long it should take for the actor to move from its current position to the target position, allowing for smooth and controlled transitions during cutscenes.</param>
         */
        void Move(ICutsceneActor actor, Vector3 target, float duration);
        /**
         * <summary>
         * Rotates the specified actor to the target rotation over the given duration. This method should be implemented to define how the actor's rotation is updated over time, allowing for smooth and controlled rotation during cutscenes.
         * The cutscene system will call this method at the appropriate time during a cutscene, passing in the relevant actor, target rotation, and duration information to allow for dynamic execution of the rotation based on the cutscene's requirements.
         * </summary>
         * <param name="actor">The cutscene actor that should be rotated. This parameter provides access to the actor's properties and methods, allowing for interaction with the actor during the execution of the rotation.</param>
         * <param name="rotation">The target rotation to which the actor should be rotated. This parameter specifies the desired final rotation of the actor after the rotation is completed.</param>
         * <param name="duration">The duration over which the rotation should occur. This parameter specifies how long it should take for the actor to rotate from its current orientation to the target rotation, allowing for smooth and controlled transitions during cutscenes.</param>
         */
        void Rotate(ICutsceneActor actor, Quaternion rotation, float duration);
    }
}