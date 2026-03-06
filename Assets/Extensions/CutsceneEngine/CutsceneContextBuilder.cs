using Unity.Cinemachine;
using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutsceneContextBuilder class is responsible for constructing a CutsceneContext, which provides access to various systems used during cutscenes.
     * It contains a static Build method that takes a MonoBehaviour as a parameter and returns a new instance of CutsceneContext with the appropriate systems initialized.
     * This class serves as a central point for creating and configuring the context that will be used during cutscene execution, ensuring that all necessary systems are properly set up.
     * </summary>
     */
    public static class CutsceneContextBuilder
    {
        /**
         * <summary>
         * Builds and returns a new instance of CutsceneContext with the motion and animation systems initialized.
         * This method allows for the centralized creation of the CutsceneContext, ensuring that all necessary systems are properly configured for use during cutscenes.
         * </summary>
         * <param name="runner">The MonoBehaviour that will be used to initialize the motion system.</param>
         * <returns>A new instance of CutsceneContext with the motion and animation systems initialized.</returns>
         */
        public static CutsceneContext Build(MonoBehaviour runner, CinemachineCamera cam = null)
        {
            return new CutsceneContext
            {
                Motion = new TransformMotionSystem(runner),
                Animation = new AnimatorAnimationSystem(),
                Camera = cam ? new CinemachineCameraSystem(runner, cam) : null
            };
        }
    }
}