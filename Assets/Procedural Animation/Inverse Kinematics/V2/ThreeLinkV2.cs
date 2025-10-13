using UnityEngine;

public class ThreeLinkV2 : MonoBehaviour {
    [System.Serializable]
    public class Joint {
        public Transform jointOrigin;
        public Transform cubeDisplay;
        public float length;
    }

    [Header("Required Variables")]
    [SerializeField] Joint[] joints = new Joint[3];
    [SerializeField] Transform origin;
    [SerializeField] Transform target;
    [Header("Tweakable Values")]
    [SerializeField] float limbThickness = 0.5f;
    [SerializeField] bool negativeSolution;

    float l1, l2, l3;

    void Start() {
        if (joints.Length != 3) {
            Debug.LogError("There must be 3 joint classes attached to this script.");
            gameObject.SetActive(false);
        }

        foreach (Joint joint in joints) {
            joint.jointOrigin.localPosition = Vector3.up * joint.length;
            joint.cubeDisplay.localPosition = Vector3.up * joint.jointOrigin.localPosition.y / 2f;
            joint.cubeDisplay.localScale = (Vector3.one - Vector3.up) * limbThickness + Vector3.up * joint.length;
        }

        l1 = joints[0].length;
        l2 = joints[1].length;
        l3 = joints[2].length;
    }


    void Update() {
        //  --------------------------------------------------------------------------------
        //                                KNOWN VARIABLES
        //  --------------------------------------------------------------------------------
        Vector3 t = target.position - origin.position - target.up * l3;
        float d = t.magnitude;

        if (d > l1 + l2 || d < Mathf.Abs(l1 - l2)) //   Exit if the target is not in range
            return;

        //  --------------------------------------------------------------------------------
        //                         SHOULDER ANGLE OFFSET CALCULATIONS
        //  --------------------------------------------------------------------------------
        //  Calculate the angle in order to point origin.right towards target.position
        float yAng = -Mathf.Atan2(t.z, t.x);
        origin.localRotation = Quaternion.Euler(0, (yAng - Mathf.PI / 2) * Mathf.Rad2Deg, 0);

        Vector3 localUp = -target.up; //    Vector we need to get angle from
        localUp = Vector3.ProjectOnPlane(localUp, t.normalized); //   Ignore the third axis of rotation

        //  Calculate the angle between the Vector3.up and localUp
        float roll = Mathf.Atan2(
            Vector3.Dot(origin.up, localUp), //    y component calculated using the projection onto Vector3.up
            Vector3.Dot(origin.right, localUp) //   x component calculated using the projection onto the perpendicular vector, origin.forward
        );

        origin.Rotate(t.normalized, -(roll - Mathf.PI / 2) * Mathf.Rad2Deg, Space.World);

        //  --------------------------------------------------------------------------------
        //                        TWO LINK INVERSE KINEMATICS SOLUTION
        //  --------------------------------------------------------------------------------
        float xProj = new Vector2(t.x, t.z).magnitude; //   Calculate the distance from (0, 0) to (t.x, t.z) to use as the x-component of the target placed inside of an 2d plane

        float angI = Mathf.Atan2(t.y, xProj); //    Calculate the angle from the x-axis to the target
        float angA = (negativeSolution ? -1 : 1) * Mathf.Acos((l1 * l1 + d * d - l2 * l2) / (2 * l1 * d)); // Calculate the angle opposite of the second link
        float angB = Mathf.PI + (negativeSolution ? 1 : -1) * Mathf.Acos((l1 * l1 + l2 * l2 - d * d) / (2 * l1 * l2)); // Calculate the exterior angle opposite of the distance

        //  Rotate the joints to be in the right place
        origin.localRotation *= Quaternion.Euler((angI + angA - Mathf.PI / 2) * Mathf.Rad2Deg, 0, 0);
        joints[0].jointOrigin.localRotation = Quaternion.Euler(-angB * Mathf.Rad2Deg, 0, 0);

        Vector3 v = (target.position - joints[1].jointOrigin.position).normalized;

        float angC = Mathf.Atan2(
            Vector3.Dot(v, -joints[0].jointOrigin.forward),
            Vector3.Dot(v, joints[0].jointOrigin.up)
        );

        joints[1].jointOrigin.localRotation = Quaternion.Euler(-angC * Mathf.Rad2Deg, 0, 0);
    }
}