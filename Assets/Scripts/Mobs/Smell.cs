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
using SIGGD.Mobs.StateMachine;

public class Smell : SerializedMonoBehaviour
{
    [SerializeField]
    private float smellRadius;
    public float SmellRadius => smellRadius;
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

    public Transform ClosestFood { get; private set; }
    public Transform PlayerTarget { get; private set; }
    public Transform ClosestPrey { get; private set; }

    public Transform ClosestPred { get; private set; }

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
            CalculateSmellIntensity();
        }
    }
    /// <summary>
    /// Checks for mobs, players, and food in a nearby range and caches targets.
    /// </summary>
    private void SmellCheck()
    {
        // One overlap sphere using a combined layer mask might be faster, but since food might be on default or another layer,
        // casting without a mask (or passing ~0) will hit everything. We can just use one physics query.
        Collider[] allColliders = Physics.OverlapSphere(transform.position, smellRadius);

        float closestPreyDist = float.MaxValue;
        Transform closestPrey = null;

        float closestFoodDist = float.MaxValue;
        Transform closestFood = null;

        float closestPredDist = float.MaxValue;
        Transform closestPred = null;

        Transform closestPlayer = null;
        float closestPlayerDist = float.MaxValue;

        foreach (Collider col in allColliders)
        {
            if (col == null || col.gameObject == gameObject) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);

            // Check if it's Player (Using playerLayer mask check via layer bitwise operation)
            if (((1 << col.gameObject.layer) & playerLayer) != 0)
            {
                if (dist < closestPlayerDist)
                {
                    closestPlayerDist = dist;
                    closestPlayer = col.transform;
                }
            }

            // Check if it's Prey
            if (col.TryGetComponent<SMPreyBrain>(out _))
            {
                if (dist < closestPreyDist)
                {
                    closestPreyDist = dist;
                    closestPrey = col.transform;
                }
            }

            // Check if it's Predator
            if (col.TryGetComponent<SMHyenaBrain>(out _))
            {
                if (dist < closestPredDist)
                {
                    closestPredDist = dist;
                    closestPred = col.transform;
                }
            }

            // Check if it's Food
            if (col.TryGetComponent<PickUpBait>(out _))
            {
                if (dist < closestFoodDist)
                {
                    closestFoodDist = dist;
                    closestFood = col.transform;
                }
            }
        }

        PlayerTarget = closestPlayer;
        if (closestPlayer != null)
        {
            playerPos = closestPlayer.position;
        }

        ClosestPrey = closestPrey;
        ClosestFood = closestFood;
        ClosestPred = closestPred;
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

            // float hierarchialWeight = weight * smellValues[i];

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
