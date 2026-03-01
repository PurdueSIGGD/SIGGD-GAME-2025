using System;
using Extensions.EventBus;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

/// <summary>
/// Manages the Apex spawn meter. A passive timer slowly fills the meter, and
/// <see cref="ApexSpawnEvent"/>s increment it by a configurable amount.
/// Once the meter exceeds <see cref="spawnThreshold"/> any subsequent event
/// (or reaching <see cref="meterMax"/>) will spawn the Apex and point it toward
/// the alert position.
/// </summary>
public class ApexSpawnSystem : MonoBehaviour
{
    #region Inspector Fields

    [Header("Spawn Settings")]
    [SerializeField] private GameObject apexPrefab;
    [SerializeField] private Transform[] possibleSpawnPoints;

    [Header("Meter Settings")]
    [Tooltip("Passive rate (units/sec) at which the meter fills on its own.")]
    [SerializeField] private float passiveFillRate = 1f;
    [Tooltip("Maximum value of the meter. Reaching this triggers an immediate spawn.")]
    [SerializeField] private float meterMax = 100f;
    [Tooltip("Once the meter is above this value any alerting event will also trigger a spawn.")]
    [SerializeField] private float spawnThreshold = 70f;

    [Header("Debug")]
    [SerializeField] private ApexSpawnEvent testSpawnEvent;

    #endregion

    #region Private State

    [SerializeField] private float meter;
    private bool apexAlive;
    private EventBinding<ApexSpawnEvent> spawnEventBinding;

    #endregion

    #region Unity Callbacks

    private void OnEnable()
    {
        spawnEventBinding = new EventBinding<ApexSpawnEvent>(OnApexSpawnEvent);
        EventBus<ApexSpawnEvent>.Register(spawnEventBinding);
    }

    private void OnDisable()
    {
        EventBus<ApexSpawnEvent>.Deregister(spawnEventBinding);
    }

    private void Update()
    {
        if (apexAlive) return;

        meter += passiveFillRate * Time.deltaTime;
        if (meter >= meterMax)
        {
            meter = meterMax;
            SpawnApex(transform.position); // no specific alert pos, spawn near center
        }
    }

    #endregion

    #region Event Handling

    private void OnApexSpawnEvent(ApexSpawnEvent spawnEvent)
    {
        if (apexAlive) return;

        meter = Mathf.Min(meter + spawnEvent.meterIncrement, meterMax);

        if (meter >= spawnThreshold)
        {
            SpawnApex(spawnEvent.targetPosition);
        }
    }

    #endregion

    #region Spawn Logic

    private void SpawnApex(Vector3 targetPosition)
    {
        if (apexAlive) return;
        apexAlive = true;
        meter = 0f;

        Vector3 spawnPosition = possibleSpawnPoints
            .OrderBy(sp => Vector3.Distance(sp.position, targetPosition))
            .First().position;

        Apex apex = Instantiate(apexPrefab, spawnPosition, Quaternion.identity).GetComponent<Apex>();
        apex.InitializeApex(targetPosition, OnApexDespawned);
    }

    private void OnApexDespawned()
    {
        apexAlive = false;
    }

    #endregion

    #region Editor Helpers

    [Button]
    public void TestSpawn()
    {
        EventBus<ApexSpawnEvent>.Raise(testSpawnEvent);
    }

    #endregion
}

/// <summary>
/// An alerting event that increments the Apex spawn meter.
/// Raise this on any action that should attract the Apex (e.g. damaging a hyena, scout calling).
/// </summary>
[Serializable]
public class ApexSpawnEvent : IEvent
{
    /// <summary>Position of the alerting action in world space.</summary>
    public Vector3 targetPosition;
    /// <summary>How much this event increments the spawn meter.</summary>
    public float meterIncrement = 20f;

    public ApexSpawnEvent(Vector3 targetPosition, float meterIncrement = 20f)
    {
        this.targetPosition = targetPosition;
        this.meterIncrement = meterIncrement;
    }
}