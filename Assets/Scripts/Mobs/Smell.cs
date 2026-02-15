using CrashKonijn.Agent.Runtime;
using SIGGD.Goap.Capabilities;
using SIGGD.Mobs;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.Windows.Speech;
using MobCensus;

public class Smell : SerializedMonoBehaviour
{
    [SerializeField]
    private float smellRadius;
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private LayerMask mobLayer;

    //public LayerMask targetMask;
    //public LayerMask smellReductionMask;

    private List<(Vector3 position, float intensity)> smellValues;

    private Vector3 playerPos;
    private Vector3 toSmellPos;
    private Vector3 awaySmellPos;

    [SerializeField]
    private float awayDistanceBase = 30.0f;

    [SerializeField]
    private float toDistanceBase = 30.0f;

    [OdinSerialize]
    [ShowInInspector]
    [DictionaryDrawerSettings(KeyLabel = "Mob Id", ValueLabel = "Intensity")]
    private Dictionary<string, float> mobSmells = MobIds.listOfMobsIds.ToDictionary(key => key, key => 0.0f);

    [SerializeField]
    private float playerSmellIntensity = 0.0f;
   
    private Vector3 position;
    void Awake()
    {

        toSmellPos = Vector3.zero;
        awaySmellPos = Vector3.zero;
        smellValues = new();
    }
    void Start()
    {
        position = Vector3.zero;
        StartCoroutine(SmellRoutine());
    }

    private IEnumerator SmellRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);

        while (true)
        {
            yield return wait;
            // For each smell, check if its intensity is low enough to be removed, otherwise it assigns a reduced intensity to the smell
            for (int i = smellValues.Count - 1; i >= 0; i--)
            {
                var (pos, intensity) = smellValues[i];
                intensity -= 0.2f * Time.deltaTime; 
                if (intensity <= 0.05f)
                {
                    smellValues.RemoveAt(i);
                }
                else
                {
                    smellValues[i] = (pos, intensity);
                }
            }

            SmellCheck();
            SmellCheckPlayer();
            CalculateSmellIntensity();
        }
    }
    /// <summary>
    /// Checks for mobs or players in a nearby range and adds a smell if the mob is not a predator
    /// </summary>
    private void SmellCheck()
    {
        smellValues.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius, mobLayer);
        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject == gameObject) continue;

            var mobSmell = collider.GetComponentInParent<MobCitizenData>();

            if (mobSmells.TryGetValue(mobSmell.GetRawData().GetMobId(), out float smellIntensity)) { 
                smellValues.Add((collider.transform.position, smellIntensity));
            }
        }
    }
    /// <summary>
    /// Checks for the presence of a player within the defined smell radius and updates the player's position if
    /// detected.
    /// </summary>
    private void SmellCheckPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius, playerLayer);
        if (hitColliders.Length < 1) return;
        playerPos = hitColliders[0].transform.position;
    }

    private void CalculateSmellIntensity()
    {
        Vector3 totalToSmellForce = Vector3.zero;
        Vector3 totalAwaySmellForce = Vector3.zero;
        float totalToSmellWeight = 0f;
        float totalAwaySmellWeight = 0f;

        // Finds the total weight and force of all the smells
        for (int i = smellValues.Count - 1; i >= 0; i--) {
            var (pos, intensity) = smellValues[i];
            Vector3 smellDir = smellValues[i].position - transform.position;

            float dist = Mathf.Max(smellDir.magnitude, 0.01f);

            if (smellDir.sqrMagnitude < 0.1f)
            {
                smellValues.RemoveAt(i);
                continue;
            }
            // The weight varies based off the inverse square of the distance
            float weight = Mathf.Pow(1f - Mathf.Clamp01(dist / smellRadius), 2f) * intensity;

            //float hierarchialWeight = weight * smellValues[i];

            if (intensity > 0)
            {
                totalToSmellForce -= smellDir.normalized * weight;
                totalToSmellWeight += weight;

            }
            else
            {
                totalAwaySmellForce += smellDir.normalized * weight;
                totalAwaySmellWeight += weight;
            }
        }
        if (totalToSmellWeight > 0f)
        {
            Vector3 averageDir = totalToSmellForce / totalToSmellWeight;
            float intensityFactor = Mathf.Clamp01(totalToSmellWeight);

            // Calculates an overall position for the smell
            toSmellPos = transform.position + averageDir.normalized * intensityFactor * toDistanceBase;
        } else
        {
            toSmellPos = transform.position;
        }
        if (totalAwaySmellWeight > 0f)
        {
            Vector3 averageDir = totalToSmellForce / totalToSmellWeight;
            float intensityFactor = Mathf.Clamp01(totalAwaySmellWeight);

            // Calculates an overall position for the smell
            awaySmellPos = transform.position + averageDir.normalized * intensityFactor * awayDistanceBase;
        }
        else
        {
            awaySmellPos = transform.position;
        }
    }
    public Vector3 GetAwaySmellPos()
    {
        return awaySmellPos;
    }
    public Vector3 GetToSmellPos()
    {
        return toSmellPos;
    }
    public Vector3 GetPlayerPos() => playerPos;
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, 2f);
    }
}
