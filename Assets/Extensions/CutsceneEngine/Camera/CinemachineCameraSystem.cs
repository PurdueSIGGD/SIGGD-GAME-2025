using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CinemachineCameraSystem class implements the ICameraSystem interface to provide camera control functionality using Unity's Cinemachine package.
     * It allows for focusing the camera on a target, moving the camera to a specific position over a given duration, and shaking the camera with specified intensity and duration.
     * This class is designed to be used within the cutscene engine to facilitate dynamic camera movements and effects during cutscenes.
     * </summary>
     */
    public class CinemachineCameraSystem : ICameraSystem
    {
        private readonly MonoBehaviour _runner;
        private readonly CinemachineCamera _cam;
        private CinemachineImpulseSource _impulseSource;

        /** <summary>The transform of the Cinemachine virtual camera. Used by MoveCameraAction.</summary> */
        public Transform CameraTransform => _cam?.transform;

        public CinemachineCameraSystem(MonoBehaviour coroutineRunner, CinemachineCamera camera)
        {
            _runner = coroutineRunner;
            _cam = camera;

            if (_cam != null)
                _impulseSource = _cam.GetComponent<CinemachineImpulseSource>()
                                 ?? _cam.gameObject.AddComponent<CinemachineImpulseSource>();
        }

        public void FocusOn(Transform target)
        {
            if (_cam == null) return;
            _cam.Follow = target;
            _cam.LookAt = target;
        }

        public void MoveTo(Vector3 position, float duration)
        {
            if (_cam == null) return;
            _runner.StartCoroutine(MoveRoutine(position, duration));
        }

        public void Shake(float intensity, float duration)
        {
            if (_impulseSource == null) return;
            // Scale the default impulse definition by the requested intensity.
            _impulseSource.DefaultVelocity = Vector3.one * intensity;
            _impulseSource.GenerateImpulse(duration);
        }

        private IEnumerator MoveRoutine(Vector3 target, float duration)
        {
            if (_cam == null) yield break;
            Vector3 start = _cam.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _cam.transform.position = Vector3.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            _cam.transform.position = target;
        }
    }
}