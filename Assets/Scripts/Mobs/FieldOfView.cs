using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Utility;

public class FieldOfView : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float viewRadius;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    [Range(0, 360)]
    public float angleRange;
    public bool canSeeTarget = true;
    public GameObject targetRef;
    public static event Action<Transform> OnPlayerDetected;
    private float loseSightDelay;
    private float lastSeenTime;
    private Vector3 lastDir;
    void Start()
    {
        targetMask = LayerMask.GetMask("Player");
        targetRef = null;
        canSeeTarget = false;
        loseSightDelay = 7f;
        lastDir = Vector3.zero;
        StartCoroutine(FOVRoutine());

    }

    // Update is called once per frame
    void Update()
    {
    }
    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.15f);

        while (true) {
            yield return wait;
            FOVCheck();
        }
    }
    private void FOVCheck()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        bool sawThisFrame = false;
        if (hitColliders.Length < 1) return;
        foreach (Collider collider in hitColliders)
        {
            {
                Transform target = collider.transform;
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                UnityUtil.DampVector3Spherical(lastDir, directionToTarget, 10f, Time.deltaTime);
                if (Vector3.Angle(transform.forward, directionToTarget) < angleRange / 2)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);
                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                    {
                        sawThisFrame = true;
                        lastSeenTime = Time.time;
                        targetRef = collider.gameObject;
                        break;
                    }
                }
                lastDir = directionToTarget;
            }
        }
        canSeeTarget = sawThisFrame || Time.time - lastSeenTime < loseSightDelay;
        if (canSeeTarget && targetRef != null)
            OnPlayerDetected?.Invoke(targetRef.transform);
        else
        {
            targetRef = null;
        }
    }
}
