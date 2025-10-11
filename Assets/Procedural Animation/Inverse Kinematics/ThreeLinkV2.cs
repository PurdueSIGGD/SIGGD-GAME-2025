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
        Vector3 t = target.position - origin.position - target.up * l3;

        float d = t.magnitude;

        float xProj = new Vector2(t.x, t.z).magnitude;
        float yAng = Mathf.Atan2(t.z, t.x);

        float angI = Mathf.Atan2(t.y, xProj);
        float angA = Mathf.Acos((l1 * l1 + d * d - l2 * l2) / (2 * l1 * d));
        float angB = Mathf.PI - Mathf.Acos((l1 * l1 + l2 * l2 - d * d) / (2 * l1 * l2));

        if (d > l1 + l2 || d < Mathf.Abs(l1 - l2))
            return;

        origin.localRotation = Quaternion.Euler(0, -yAng * Mathf.Rad2Deg, (angI + angA - Mathf.PI / 2) * Mathf.Rad2Deg);
        joints[0].jointOrigin.localRotation = Quaternion.Euler(0, 0, -angB * Mathf.Rad2Deg);

        float d_2 = (joints[0].jointOrigin.position - target.position).magnitude;
        float angC = Mathf.PI - Mathf.Acos((l2 * l2 + l3 * l3 - d_2 * d_2) / (2 * l2 * l3));

        joints[1].jointOrigin.localRotation = Quaternion.Euler(0, 0, -angC * Mathf.Rad2Deg);
    }

    void OnDrawGizmos() {
        Gizmos.DrawSphere(target.position - target.up * l3, 0.1f);
    }
}
