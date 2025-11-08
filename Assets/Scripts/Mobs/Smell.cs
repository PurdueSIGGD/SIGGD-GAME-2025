using SIGGD.Mobs;
using SIGGD.Goap.Capabilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;

public class Smell : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float smellRadius;
    public LayerMask targetMask;
    public LayerMask smellReductionMask;
    public float targetSmellIntensity;
    private HungerBehaviour HungerBehaviour;
    private PreyBehaviour PreyBehaviour;
    private bool isPrey;
    private bool isPredator;
    private List<Vector3> preyPositions;
    private List<Vector3> predatorPositions;
    private List<(Vector3 position, float decay)> smellValues;
    private Vector3 playerPos;
    private LayerMask playerLayer;
    private LayerMask mobLayer;
    private Vector3 position;
    private Vector3 smellPos;
    void Awake()
    {
        HungerBehaviour = GetComponent<HungerBehaviour>();
        PreyBehaviour = GetComponent<PreyBehaviour>();
        playerLayer = LayerMask.NameToLayer("Player");
        mobLayer = LayerMask.NameToLayer("Mob");
        smellPos = Vector3.zero;
        smellValues = new();
    }
    void Start()
    {
        position = Vector3.zero;
        isPrey = gameObject.CompareTag("Prey");
        isPredator = gameObject.CompareTag("Predator");
        StartCoroutine(SmellRoutine());
    }

    void Update()
    {

    }
    private IEnumerator SmellRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);

        while (true)
        {
            yield return wait;
            SmellCheck();
            SmellCheckPlayer();
            CalculateSmellIntensity();
        }
    }
    private void SmellCheck()
    {
        smellValues.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius, playerLayer | mobLayer);
        foreach (Collider collider in hitColliders)
        {
            if (collider.GetComponentInParent<AgentHuntBehaviour>() != null) { 
                smellValues.Add((collider.transform.position, 0.7f));
            }
            //Transform target = collider.transform;
            //if (collider.GetComponentInParent<MobIds>)
        }
    }
    private void SmellCheckPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius, playerLayer);
        if (hitColliders.Length < 1) return;
        playerPos = hitColliders[0].transform.position;
    }
    private void CalculateSmellIntensity()
    {
        float safeDistanceThreshold = 30f;
        Vector3 totalForce = Vector3.zero;
        float totalWeight = 0f;

        for (int i = 0; i < smellValues.Count; i++) {
            smellValues[i] = (smellValues[i].position * smellValues[i].decay, smellValues[i].decay);
            if (smellValues[i].position.sqrMagnitude < 3f) smellValues.RemoveAt(i);
            Vector3 toSmell = smellValues[i].position - transform.position;
            float dist = Mathf.Max(toSmell.magnitude, 1f);

            float weight = Mathf.Pow(1f - Mathf.Clamp01(dist / smellRadius), 2f);

            totalForce += toSmell.normalized * weight;
            totalWeight += weight;
        }
        Vector3 pos = Vector3.zero;
        if (totalWeight > 0f)
        {
            Vector3 averageDir = totalForce / totalWeight;
            float intensity = Mathf.Clamp01(totalWeight);

            pos = transform.position + averageDir.normalized * intensity * safeDistanceThreshold;
        }
        smellPos = pos;
    }
    public Vector3 GetSmellPos() => isPredator ? smellPos + playerPos * 5f : smellPos;
    public Vector3 GetPlayerPos() => playerPos;
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, 2f);
    }
}
