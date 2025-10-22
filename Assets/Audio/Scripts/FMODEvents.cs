using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using System;
using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using FMOD.Studio;

public class FMODEvents : SerializedMonoBehaviour
{
    [field: Header("Ambiance")]
    [field: SerializeField] public EventReference ambience { get; private set; }

    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference enemyDeath { get; private set; }

    [field: Header("Footsteps")]
    [field: SerializeField] public EventReference footsteps { get; private set; }

    [field: Header("Background Music")]
    [field: SerializeField] public EventReference music { get; private set; }

    [field: Header("Server Room Noise")]
    [field: SerializeField] public EventReference serverNoise { get; private set; }

    [SerializeField] private bool initialized = false;
    public static FMODEvents instance { get; private set; }

    public List<string> bankNames = new List<string>();

    [OdinSerialize] private Dictionary<string, EventReference> soundEvents = new Dictionary<string, EventReference>();

    // holds requests for event refs when they are called before soundEvents is filled 
    [OdinSerialize] private Dictionary<string, List<Action<EventReference>>> requestQueue = new Dictionary<string, List<Action<EventReference>>>();

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Too many FMODEvents instances!");
        }

        instance = this;

        // Load all banks immediately
        foreach (string name in bankNames)
        {
            RuntimeManager.LoadBank(name, true);
        }
    }

    private void Start()
    {
        StartCoroutine(LoadSoundEventsPostBankLoading());
    }

    private IEnumerator LoadSoundEventsPostBankLoading()
    {
        yield return null; // Give FMOD a frame to load banks

        foreach (string name in bankNames)
        {
            string filePath = "bank:/" + name.Replace(".bank", "");
            if (RuntimeManager.StudioSystem.getBank(filePath, out Bank bank) != FMOD.RESULT.OK)
            {
                Debug.LogError("Failed to get bank: " + filePath);
                continue;
            }

            bank.getEventList(out EventDescription[] eventDescriptions);

            foreach (EventDescription desc in eventDescriptions)
            {
                desc.getPath(out string eventPath);
                EventReference eventRef = EventReference.Find(eventPath);

                soundEvents[eventPath.Replace("event:/", "")] = eventRef;
            }
        }

        initialized = true;

        // process queued requests
        foreach (var request in requestQueue)
        {
            if (soundEvents.TryGetValue(request.Key, out var eventRef))
            {
                foreach (var action in request.Value)
                    action.Invoke(eventRef);
            }
        }
        requestQueue.Clear();

        Debug.Log($"FMODEvents initialized: {soundEvents.Count} events loaded.");
    }

    /// <summary>
    /// Returns an EventReference immediately if FMODEvents is initialized, otherwise queues it.
    /// </summary>
    public void GetReadyEvent(string key, Action<EventReference> action)
    {
        if (initialized)
        {
            if (soundEvents.TryGetValue(key, out var eventRef))
                action.Invoke(eventRef);
            else
                Debug.LogError($"FMODEvents: Event '{key}' not found.");
        }
        else
        {
            if (!requestQueue.ContainsKey(key))
                requestQueue[key] = new List<Action<EventReference>>();

            requestQueue[key].Add(action);
        }
    }

    /// <summary>
    /// Assigns a playable EventInstance to a callback. Handles queuing automatically.
    /// </summary>
    public void AssignEventTo(string key, Action<EventInstance> action)
    {
        GetReadyEvent(key, (eventRef) =>
        {
            if (!eventRef.IsNull)
            {
                EventInstance instance = AudioManager.instance.CreateEventInstance(eventRef);
                action.Invoke(instance);
            }
            else
            {
                Debug.LogError($"FMODEvents: EventReference for key '{key}' is null.");
            }
        });
    }
}
