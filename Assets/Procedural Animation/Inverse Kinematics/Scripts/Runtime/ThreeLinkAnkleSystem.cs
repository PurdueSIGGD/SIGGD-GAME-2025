using UnityEngine;

namespace ProceduralAnimation.Runtime {
    /// <summary>
    /// Solver for a 3 link arm where the last link is an ankle/wrist that is just offset by taking the target's up vector and multiplying it by -l3.
    /// This solver does not preserve the roll of the first two links when calculating the last link's orientation.
    /// The math is almost the same as ThreeLinkV2 so refer to those comments.
    /// </summary>
    public class ThreeLinkAnkleSystem : MonoBehaviour {
        [Header("Public Variables")]
        public Transform origin;
        public Transform target;
        public Transform raycastDir;
        [Header("Joints")]
        [SerializeField] Transform joint1;
        [SerializeField] Transform joint2;
        [SerializeField] Transform joint3;
        [Header("Link Lengths")]
        [SerializeField] float l1;
        [SerializeField] float l2;
        public float l3;
        [Header("Debug")]
        [SerializeField] bool activated;
        [SerializeField] bool drawBox;
        [HideInInspector] public bool lerping = false;
        public bool offsetWithParent = true;

        void OnDrawGizmos() {
            Vector3 t = target.position - origin.position + -target.up * l3; // target position relative to the origin position
            float d = t.magnitude; // distance between origin and target

            // editor display stuff
            if (!drawBox) {
                Gizmos.DrawLine(origin.position, joint1.position);
                Gizmos.DrawLine(joint1.position, joint2.position);
                Gizmos.DrawLine(joint2.position, joint3.position);
            } else {
                DrawBoxBetweenPoints(origin, joint1, 0.25f);
                DrawBoxBetweenPoints(joint1, joint2, 0.25f);
                DrawBoxBetweenPoints(joint2, joint3, 0.25f);
            }

            // set the joint distances to be equal to the link lengths
            joint1.localPosition = Vector3.up * l1;
            joint2.localPosition = Vector3.up * l2;
            joint3.localPosition = Vector3.up * l3;

            // calculate the angles and whatnot
            // these lines are for mapping the 3d world onto a 2d plane
            float x = new Vector2(t.x, t.z).magnitude;
            float yAng = Mathf.Atan2(t.z, t.x); // make using transform.up instead

            // solving the triangle
            float angOffset = Mathf.Atan2(t.y, x);
            float ang1 = Mathf.Acos((l1 * l1 + d * d - l2 * l2) / (2 * l1 * d));
            float ang2 = Mathf.Acos((l1 * l1 + l2 * l2 - d * d) / (2 * l1 * l2));

            // error checks
            if (float.IsNaN(ang1) || float.IsNaN(ang2))
                return;

            // applying rotation
            origin.localEulerAngles = Vector3.forward * Mathf.Rad2Deg * (Mathf.PI / 2 - angOffset - ang1) + Vector3.up * Mathf.Rad2Deg * (Mathf.PI - yAng);
            joint1.localEulerAngles = Vector3.forward * Mathf.Rad2Deg * (Mathf.PI - ang2);
            joint2.up = target.position - origin.position - t;
        }

        // literally just draws a box lmao
        void DrawBoxBetweenPoints(Transform start, Transform end, float size) {
            Vector3[] offset = new Vector3[] {
            start.forward + start.right,
            start.forward - start.right,
            -start.forward - start.right,
            -start.forward + start.right,
        };
            for (int i = 0; i < 4; i++) {
                Gizmos.DrawLine(offset[i] * size + start.position, offset[i] * size + end.position);
                Gizmos.DrawLine(offset[i] * size + start.position, offset[i + 1 > 3 ? 0 : i + 1] * size + start.position);
                Gizmos.DrawLine(offset[i] * size + end.position, offset[i + 1 > 3 ? 0 : i + 1] * size + end.position);
            }
        }
    }
}