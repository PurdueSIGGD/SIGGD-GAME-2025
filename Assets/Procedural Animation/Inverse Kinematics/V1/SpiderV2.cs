using UnityEngine;

public class SpiderV2 : MonoBehaviour {
    [System.Serializable]
    public class Leg {
        public int legGroup = 0;
        public Transform IKTarget;
        public Transform groundTarget;
        public Transform groundRayTarget;
        public Transform shoulderPivot;
        public ThreeLinkAnkleSystem system;
    }

    [SerializeField] Transform body;
    [SerializeField] Leg[] legs;
    [SerializeField] float floorDist = 3f;
    [SerializeField] float raycastHeight = 3f;

    void OnDrawGizmos() {
        foreach (Leg leg in legs) {
            RaycastHit hit;
            if (Physics.Raycast(leg.groundTarget.position + leg.groundTarget.up * raycastHeight, -leg.groundTarget.up, out hit, raycastHeight + 1f)) {
                leg.groundRayTarget.position = hit.point;
                leg.groundRayTarget.up = -hit.normal;
            }
        }

        CalculateBodyTransform();
    }

    void CalculateBodyTransform() {
        float height = 0;
        foreach (Leg leg in legs) {
            height += leg.system.l3 + (leg.system.l3 * leg.IKTarget.up).y;
        }

        body.transform.position = new Vector3(transform.position.x, floorDist + height / 4f, transform.position.z);
    }
}
