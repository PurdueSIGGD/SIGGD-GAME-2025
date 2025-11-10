using Sirenix.OdinInspector;
using UnityEngine;

namespace ProceduralAnimation.Runtime {
    /// <summary>
    /// Solver for a 3 link arm where the last link is an ankle/wrist that is just offset by taking the target's up vector and multiplying it by -l3.
    /// This solve does preserve the roll of the first two links by rolling the entire system instead.
    /// </summary>
    public class ThreeLinkV2 : MonoBehaviour {
        [System.Serializable]
        public class Joint {
            public Transform jointOrigin; //    This is the point that gets rotated
            public Transform cubeDisplay; //    This one is technically optional (i'll make it optional later)
            public float length; // The distance between this joint and the next/previous.
        }

        [Header("Required Variables")]
        [SerializeField] Transform origin; //  The origin position, the start of the first link
        public Transform target; // The desired location of the end effector
        public Joint[] joints = new Joint[3]; //    The list of all joints. The length of this array must be 3 or the script won't work.
        [Header("Tweakable Values")]
        [SerializeField] bool useCubesAsLimbs = false; //   Toggle temporary display made by scaled cubes
        [SerializeField, ShowIf("useCubesAsLimbs")] float limbThickness = 0.5f; // Only used if cubeDisplay is on
        [SerializeField] bool negativeSolution; //  Whether or not to use the positive solution of the two link IK
        float l1, l2, l3; //    Used to make the math easier to understand

        void Start() {
            //  Check if there are only 3 joints.
            if (joints.Length != 3) {
                Debug.LogError("There must be 3 joint classes attached to this script.");
                gameObject.SetActive(false);
            }

            //  Initialize each joint
            foreach (Joint joint in joints) {
                joint.jointOrigin.localPosition = Vector3.up * joint.length;
                joint.cubeDisplay.localPosition = Vector3.up * joint.jointOrigin.localPosition.y / 2f;

                //  Scale the cube up if useCubesAsLimbs is checked
                if (useCubesAsLimbs)
                    joint.cubeDisplay.localScale = (Vector3.one - Vector3.up) * limbThickness + Vector3.up * joint.length;
            }

            //  Made for easier code reading
            l1 = joints[0].length;
            l2 = joints[1].length;
            l3 = joints[2].length;
        }


        void Update() {
            //  --------------------------------------------------------------------------------
            //                                KNOWN VARIABLES
            //  --------------------------------------------------------------------------------
            //  Calculate the position of the second joint after the two link IK solver is done, also offset by origin.position
            Vector3 t = target.position - origin.position - target.up * l3;

            //  Calculate distance
            float d = t.magnitude;

            //   Exit if the target is not in range
            if (d > l1 + l2 || d < Mathf.Abs(l1 - l2))
                return;

            //  --------------------------------------------------------------------------------
            //                         SHOULDER ANGLE OFFSET CALCULATIONS
            //  --------------------------------------------------------------------------------
            //  Calculate the angle in order to point origin.forward towards target.position
            float yAng = -Mathf.Atan2(t.z, t.x);
            origin.rotation = Quaternion.Euler(0, (yAng - Mathf.PI / 2) * Mathf.Rad2Deg, 0);

            Vector3 localUp = -target.up; //    Vector we need to get angle from
            localUp = Vector3.ProjectOnPlane(localUp, t.normalized); //   Ignore the third axis of rotation

            //  Calculate the angle between the Vector3.up and localUp
            float roll = Mathf.Atan2(
                Vector3.Dot(origin.up, localUp), //    y component calculated using the projection onto Vector3.up
                Vector3.Dot(origin.right, localUp) //   x component calculated using the projection onto the perpendicular vector, origin.forward
            );

            //  Rotate the origin using calculated roll
            origin.Rotate(t.normalized, -(roll - Mathf.PI / 2) * Mathf.Rad2Deg, Space.World);

            //  --------------------------------------------------------------------------------
            //                        TWO LINK INVERSE KINEMATICS SOLUTION
            //  --------------------------------------------------------------------------------
            float xProj = new Vector2(t.x, t.z).magnitude; //   Calculate the distance from (0, 0) to (t.x, t.z) to use as the x-component of the target placed inside of an 2d plane

            float angI = Mathf.Atan2(t.y, xProj); //    Calculate the angle from the x-axis to the target
            float angA = (negativeSolution ? -1 : 1) * Mathf.Acos((l1 * l1 + d * d - l2 * l2) / (2 * l1 * d)); // Calculate the angle opposite of the second link
            float angB = Mathf.PI + (negativeSolution ? 1 : -1) * Mathf.Acos((l1 * l1 + l2 * l2 - d * d) / (2 * l1 * l2)); // Calculate the exterior angle opposite of the distance

            //  Rotate the joints to be in the right place
            origin.rotation *= Quaternion.Euler((angI + angA - Mathf.PI / 2) * Mathf.Rad2Deg, 0, 0);
            joints[0].jointOrigin.localRotation = Quaternion.Euler(-angB * Mathf.Rad2Deg, 0, 0);


            //  Calculate the angle to bring the last joint down by to reach target
            float angC = Mathf.Atan2(
                Vector3.Dot(target.up, -joints[0].jointOrigin.forward),
                Vector3.Dot(target.up, joints[0].jointOrigin.up)
            );

            //  Bring last joint down by angC
            joints[1].jointOrigin.localRotation = Quaternion.Euler(-angC * Mathf.Rad2Deg, 0, 0);
        }
    }
}