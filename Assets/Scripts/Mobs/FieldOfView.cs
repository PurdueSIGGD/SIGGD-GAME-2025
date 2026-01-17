using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Utility;
using System.Collections.Generic;
using Unity.Collections;
using CrashKonijn.Goap.Runtime;
using Unity.Hierarchy;

public class FieldOfView : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float viewRadius;
    public LayerMask playerMask;
    public LayerMask mobMask;
    public LayerMask obstacleMask;
    [Range(0, 360)]
    public float angleRange;
    public GameObject targetRef;
    private float loseSightDelay;
    private float lastSeenTime;
    private Dictionary<Transform, DetectedTarget> detected = new();
    private List<GameObject> seenTargets = new List<GameObject>();

    struct DetectedTarget
    {
        public GameObject gameObject;
        public int hierarchy;
        public float lastSeenTime;
        public DetectedTarget(GameObject gameObject, int hierarchy, float lastSeenTime)
        {
            this.gameObject = gameObject;
            this.hierarchy = hierarchy;
            this.lastSeenTime = lastSeenTime;
        }
    }
    void Start()
    {   
        playerMask = LayerMask.GetMask("Player");
        mobMask = LayerMask.GetMask("Mob");
        targetRef = null;
        loseSightDelay = 7f;
        StartCoroutine(FOVRoutine());

    }
    void Update()
    {
    }
    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.15f);

        while (true) {
            yield return wait;
            FOVCheck();
            CleanExpiredTargets();
        }
    }
    private void FOVCheck()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewRadius, playerMask | mobMask);
        if (hitColliders.Length == 0) return;
        for (int i = 0; i < hitColliders.Length; i++)
        {
            {
                Transform target = hitColliders[i]?.transform;
                if (target == null) continue;
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, directionToTarget) > angleRange / 2f)
                    continue;

                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (Physics.Raycast(transform.position + Vector3.up, directionToTarget, distanceToTarget, obstacleMask))
                    continue;

                detected[target] = new DetectedTarget(hitColliders[i].gameObject, 1, Time.time);
            }
        }
    }
    private void CleanExpiredTargets()
    {
        seenTargets.Clear();
        PlayerTarget = null;
        List<Transform> toRemove = new();
        foreach (KeyValuePair<Transform, DetectedTarget> pair in detected)
        {
            if (Time.time - pair.Value.lastSeenTime > loseSightDelay)
            {
                toRemove.Add(pair.Key);
                continue;
            }
            seenTargets.Add(pair.Value.gameObject);
            if (pair.Value.gameObject.CompareTag("Player")) PlayerTarget = pair.Value.gameObject;
        }
        foreach (var v in toRemove)
            detected.Remove(v);
    }
    public GameObject PlayerTarget { get; private set; }
    public List<GameObject> GetSeenTargets() => seenTargets;
}