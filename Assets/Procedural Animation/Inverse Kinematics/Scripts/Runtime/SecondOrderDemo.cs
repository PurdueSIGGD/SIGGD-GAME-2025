using Sirenix.OdinInspector;
using UnityEngine;

namespace ProceduralAnimation.Runtime.Dynamics {
    public class SecondOrderDemo : MonoBehaviour {
        [SerializeField] Transform target;
        [SerializeField, InlineEditor] SecondOrderSettings settings;
        SecondOrderDynamics positionFilter;
        SecondOrderDynamics rotationFilter;

        void Start() {
            positionFilter = new SecondOrderDynamics(settings.f, settings.z, settings.r, transform.position);
            rotationFilter = new SecondOrderDynamics(settings.f, settings.z, settings.r, transform.forward);
        }

        void Update() {
            positionFilter.ComputeKValues(settings.f, settings.z, settings.r);
            Vector3 yPos = positionFilter.Update(Time.deltaTime, target.position);
            transform.position = yPos;

            rotationFilter.ComputeKValues(settings.f, settings.z, settings.r);
            Vector3 yRot = rotationFilter.Update(Time.deltaTime, target.forward);
            transform.forward = yRot;
        }
    }
}
