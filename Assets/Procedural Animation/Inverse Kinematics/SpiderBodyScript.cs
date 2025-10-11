using System.Collections;
using UnityEngine;

public class SpiderBodyScript : MonoBehaviour {
    // someone please make this better lmao
    [System.Serializable]
    public class LegGroup {
        public ThreeLinkAnkleSystem[] legs;
        public Transform[] targets;
    }

    [SerializeField] LegGroup[] legGroups = new LegGroup[] { };
    [Header("Parameters")]
    [SerializeField] float lerpTime = 0.5f;
    [SerializeField] float maxLegDelta = 0.5f;
    [SerializeField] float maxRaycastDist = 2f;
    [Header("Debug")]
    [SerializeField] bool activated;
    [SerializeField] bool showRaycastDirs;
    [SerializeField] int currMovingIndex = 0;
    [SerializeField] bool groupMoving;

    void OnDrawGizmos() {
        if (!activated)
            return;
        for (int i = 0; i < legGroups.Length; i++) {
            ThreeLinkAnkleSystem[] legs = legGroups[i].legs;
            Transform[] targets = legGroups[i].targets;
            for (int j = 0; j < legs.Length; j++) {
                // puts the raycast target onto the ground
                RaycastHit hit;
                if (Physics.Raycast(legs[j].origin.position, legs[j].raycastDir.up, out hit, maxRaycastDist)) {
                    targets[j].position = hit.point;
                    targets[j].up = -hit.normal;
                }

                // lerp the IK target position towards the raycast target position 
                if (Vector3.Distance(targets[j].position, legs[j].target.position) > maxLegDelta
                    && !legs[j].lerping && (currMovingIndex == i || !groupMoving))
                    StartCoroutine(LerpTarget(legs[j], targets[j], i));

                // used to show the raycast directions
                // this is controlled by the raycast dir transform under each leg 
                if (showRaycastDirs)
                    Gizmos.DrawLine(legs[j].origin.position, legs[j].origin.position + legs[j].raycastDir.up * maxRaycastDist);
            }
        }
    }

    // lerp using time
    IEnumerator LerpTarget(ThreeLinkAnkleSystem leg, Transform raycastTarget, int groupIndex) {
        float startTime = Time.time;
        leg.lerping = true;
        groupMoving = true;

        while (Time.time < startTime + lerpTime) {
            leg.target.position = Vector3.Lerp(leg.target.position, raycastTarget.position, (Time.time - startTime) / lerpTime);
            leg.target.up = Vector3.Lerp(leg.target.up, raycastTarget.up, (Time.time - startTime) / lerpTime);
            yield return null;
        }

        groupMoving = false;
        leg.lerping = false;

        currMovingIndex = ++groupIndex == legGroups.Length ? 0 : groupIndex;
    }
}
