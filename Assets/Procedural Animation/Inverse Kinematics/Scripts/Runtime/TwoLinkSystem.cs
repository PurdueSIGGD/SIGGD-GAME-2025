using UnityEngine;

namespace ProceduralAnimation.Runtime {
    /// <summary>
    /// 3D Two link solver, math is similar to last block in ThreeLinkV2
    /// </summary>
    public class TwoLinkSystem : MonoBehaviour {
        [SerializeField] Transform origin;
        [SerializeField] Transform joint1;
        [SerializeField] Transform joint2;
        [SerializeField] Transform target;
        [SerializeField] float l1;
        [SerializeField] float l2;
        [SerializeField] bool activated;

        Vector3 t => target.position - origin.position;

        void OnDrawGizmos() {
            float d = t.magnitude;

            // exit if too far/close
            if (!activated || d > l1 + l2 || d < Mathf.Abs(l1 - l2))
                return;

            joint1.localPosition = Vector3.up * l1;
            joint2.localPosition = Vector3.up * l2;

            float x = new Vector2(t.x, t.z).magnitude;
            float yAng = Mathf.Atan2(t.z, t.x);

            float angOffset = Mathf.Atan2(t.y, x);
            float ang1 = Mathf.Acos((l1 * l1 + d * d - l2 * l2) / (2 * l1 * d));
            float ang2 = Mathf.Acos((l1 * l1 + l2 * l2 - d * d) / (2 * l1 * l2));

            origin.localEulerAngles = Vector3.forward * radToDeg(Mathf.PI / 2 - angOffset - ang1) + Vector3.up * radToDeg(Mathf.PI - yAng);
            joint1.localEulerAngles = Vector3.forward * radToDeg(Mathf.PI - ang2);

            Gizmos.DrawLine(origin.position, joint1.position);
            Gizmos.DrawLine(joint1.position, joint2.position);
        }

        float radToDeg(float rad) {
            return rad * 180 / Mathf.PI;
        }
    }
}