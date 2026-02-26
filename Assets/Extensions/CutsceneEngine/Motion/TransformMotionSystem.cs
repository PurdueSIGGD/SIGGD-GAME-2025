using System.Collections;
using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The TransformMotionSystem class implements the IMotionSystem interface to provide functionality for moving and rotating cutscene actors using their Transform components.
     * It utilizes Unity's coroutine system to smoothly interpolate the position and rotation of actors over a specified duration, allowing for dynamic and visually appealing cutscene animations.
     * The Move method moves an actor to a target position, while the Rotate method rotates an actor to a target orientation, both over a given duration. This class requires a MonoBehaviour instance to run the coroutines.
     * </summary>
     */
    public class TransformMotionSystem : IMotionSystem
    {
        private MonoBehaviour runner;
        
        public TransformMotionSystem(MonoBehaviour coroutineRunner)
        {
            runner = coroutineRunner;
        }
        
        public void Move(ICutsceneActor actor, Vector3 target, float duration)
        {
            runner.StartCoroutine(MoveRoutine(actor.GetTransform(), target, duration));
        }

        private IEnumerator MoveRoutine(Transform t, Vector3 target, float duration)
        {
            Vector3 start = t.position;
            float time = 0f;

            while (time < duration)
            {
                time += UnityEngine.Time.deltaTime;
                t.position = Vector3.Lerp(start, target, time / duration);
                yield return null;
            }
        }
        
        public void Rotate(ICutsceneActor actor, Quaternion rot, float duration)
        {
            runner.StartCoroutine(RotateRoutine(actor.GetTransform(), rot, duration));
        }

        private IEnumerator RotateRoutine(Transform t, Quaternion rot, float duration)
        {
            Quaternion start = t.rotation;
            float time = 0f;

            while (time < duration)
            {
                time += UnityEngine.Time.deltaTime;
                t.rotation = Quaternion.Slerp(start, rot, time / duration);
                yield return null;
            }
        }
    }
}