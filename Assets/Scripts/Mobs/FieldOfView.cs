using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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
    void Start()
    {
        StartCoroutine(FOVRoutine());

    }

    // Update is called once per frame
    void Update()
    {
    }
    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true) {
            yield return wait;
            FOVCheck();
        }
    }
    private void FOVCheck()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewRadius);
        if (hitColliders.Length > 0 )
        {
            Transform target = hitColliders[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < angleRange / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    canSeeTarget = true;
                } else
                {
                    canSeeTarget = false;
                }
            } else
            {
                canSeeTarget = false;
            }
        }
        if (canSeeTarget)
        {
            canSeeTarget = false;
        }

    }
}
