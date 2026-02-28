using CrashKonijn.Agent.Runtime;
using MobCensus;
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
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.Windows.Speech;
using static UnityEditor.PlayerSettings;

public class Smell : SerializedMonoBehaviour
{
    [SerializeField]
    private float smellRadius;
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private LayerMask mobLayer;
    [SerializeField]
    private LayerMask lureLayer;

    //public LayerMask targetMask;
    //public LayerMask smellReductionMask;

    [SerializeField]
    private Dictionary<GameObject, SmellValue> smellValues;

    private Vector3 playerPos;
    private Vector3 toSmellPos;
    private Vector3 awaySmellPos;

    public struct SmellValue
    {
        public Vector3 position;
        public float intensity;
    }

    [SerializeField]
    private float awayDistanceBase = 30.0f;

    [SerializeField]
    private float toDistanceBase = 80.0f;

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
        smellValues = new Dictionary<GameObject, SmellValue>();
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
            foreach (var key in smellValues.Keys.ToList())
            {
                var value = smellValues[key];
                value.intensity -= 0.2f * 0.5f; 
                if (value.intensity <= 0.05f)
                {
                    smellValues.Remove(key);
                }
                else
                {
                    smellValues[key] = value;
                }
            }

            SmellCheck();
            SmellCheckPlayer();
            SmellCheckLures();
            CalculateSmellIntensity();
        }
    }
    /// <summary>
    /// Checks for mobs or players in a nearby range and adds a smell if the mob is not a predator
    /// </summary>
    private void SmellCheck()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius, mobLayer);
        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject == gameObject) continue;

            GameObject key = collider.gameObject;
            Vector3 pos = collider.transform.position;

            var agentData = collider.GetComponentInParent<AgentData>();

            if (agentData == null) continue;

            if (!mobSmells.TryGetValue(agentData.GetMobId(), out float smellIntensity))
            {
                continue;
            }
            if (smellValues.TryGetValue(key, out var existingSmell))
            {
                existingSmell.position = pos;
                existingSmell.intensity = smellIntensity;
                smellValues[key] = existingSmell;
            } else {
                smellValues.Add(key, new SmellValue
                {
                    position = pos,
                    intensity = smellIntensity
                });
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
        List<GameObject> toRemove = null;

        foreach (var kvp in smellValues) {
            var source = kvp.Key;
            var smell = kvp.Value;

            if (source == null)
            {
                (toRemove ??= new List<GameObject>()).Add(kvp.Key);
                continue;
            }
            Vector3 smellDir = smell.position - transform.position;

            float dist = Mathf.Max(smellDir.magnitude, 0.01f);

            if (smellDir.sqrMagnitude < 0.1f)
            {
                continue;
            }
            // The weight varies based off the inverse square of the distance
            float weight = Mathf.Pow(1f - Mathf.Clamp01(dist / smellRadius), 2f) * smell.intensity;


            if (smell.intensity > 0)
            {
                totalToSmellForce += smellDir.normalized * weight;
                totalToSmellWeight += weight;

            }
            else
            {
                totalAwaySmellForce -= smellDir.normalized * weight;
                totalAwaySmellWeight += weight;
            }
        }
        if (toRemove != null)
        {
            foreach (var k in toRemove)
            {
                smellValues.Remove(k);
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
            Vector3 averageDir = totalAwaySmellForce / totalAwaySmellWeight;
            float intensityFactor = Mathf.Clamp01(totalAwaySmellWeight);

            // Calculates an overall position for the smell
            awaySmellPos = transform.position + averageDir.normalized * intensityFactor * awayDistanceBase;
        }
        else
        {
            awaySmellPos = transform.position;
        }
    }

    private void SmellCheckLures()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius, lureLayer);
        foreach (Collider collider in hitColliders)
        {
            if (collider == null) continue;
            var lure = collider.GetComponentInParent<Lure>();
            if (lure == null) continue;
            GameObject key = collider.gameObject;
            Vector3 pos = collider.transform.position;
            float smellIntensity = lure.intensity;
            if (smellValues.TryGetValue(key, out var existingSmell))
            {
                existingSmell.position = pos;
                existingSmell.intensity = smellIntensity;
                smellValues[key] = existingSmell;
            }
            else
            {
                smellValues.Add(key, new SmellValue
                {
                    position = pos,
                    intensity = smellIntensity
                });
            }

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
        Gizmos.DrawWireSphere(toSmellPos, 10f);
    }
}
