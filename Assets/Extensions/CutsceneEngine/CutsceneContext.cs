namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutsceneContext class provides a context for cutscene actions, containing references to the motion system, animation system, and camera system.
     * This context is passed to cutscene actions when they are executed, allowing them to access and manipulate these systems as needed to achieve the desired effects during cutscenes.
     * By providing a centralized context for these systems, the CutsceneContext class enables cutscene actions to interact with the game's motion, animation, and camera systems in a consistent and organized manner.
     * </summary>
     */
    public class CutsceneContext
    {
        public IMotionSystem Motion;
        public IAnimationSystem Animation;
        public ICameraSystem Camera;
    }
}