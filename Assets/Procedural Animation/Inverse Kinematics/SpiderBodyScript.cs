using System.Collections;
using UnityEngine;

public class SpiderBodyScript : MonoBehaviour
{
    // someone please make this better lmao
    [SerializeField] ThreeLinkAnkleSystem[] legs = new ThreeLinkAnkleSystem[4];
    [SerializeField] Transform[] targets;
    [Header("Parameters")]
    [SerializeField] float lerpTime = 0.5f;
    [SerializeField] float maxLegDelta = 0.5f;
    [SerializeField] float maxRaycastDist = 2f;
    [Header("Debug")]
    [SerializeField] bool showRaycastDirs;

    void OnDrawGizmos()
    {
        if (targets.Length != legs.Length)
            return;

        for (int i = 0; i < legs.Length; i++)
        {
            // puts the raycast target onto the ground
            RaycastHit hit;
            if (Physics.Raycast(legs[i].origin.position, legs[i].raycastDir.up, out hit, maxRaycastDist))
            {
                targets[i].position = hit.point;
                targets[i].up = -hit.normal;
            }

            // lerp the IK target position towards the raycast target position 
            if (Vector3.Distance(targets[i].position, legs[i].target.position) > maxLegDelta && !legs[i].lerping)
                StartCoroutine(LerpTarget(legs[i], targets[i]));

            // used to show the raycast directions
            // this is controlled by the raycast dir transform under each leg 
            if (showRaycastDirs)
                Gizmos.DrawLine(legs[i].origin.position, legs[i].origin.position + legs[i].raycastDir.up * maxRaycastDist);
        }
    }

    // lerp using time
    IEnumerator LerpTarget(ThreeLinkAnkleSystem leg, Transform raycastTarget)
    {
        float startTime = Time.time;
        leg.lerping = true;

        while (Time.time < startTime + lerpTime)
        {
            leg.target.position = Vector3.Lerp(leg.target.position, raycastTarget.position, (Time.time - startTime) / lerpTime);
            yield return null;
        }

        leg.lerping = false;
    }
}
