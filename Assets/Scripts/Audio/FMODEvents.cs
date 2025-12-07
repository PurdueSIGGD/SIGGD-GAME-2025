using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using FMOD.Studio;
using System.Collections;
using System;

/// <summary>
/// Handles loading FMOD banks and allows accessing FMOD events through string names
/// </summary>
public class FMODEvents : Singleton<FMODEvents>
{
    [HideInInspector] public bool Initialized { get; private set; }
    [SerializeField] List<string> bankNames = new(); // if you put a bank that doesnt exist into this list it will break the whole loop    

    public Dictionary<string, EventReference> soundEvents = new();
    private Coroutine loadroutine;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        ReloadAudioBanks();
    }

    /// <summary>
    /// Force reload audio banks from FMOD. Disable referencing new events while loading.
    /// </summary>
    public void ReloadAudioBanks()
    {
        loadroutine ??= StartCoroutine(LoadBanksCoroutine());
    }

    /// <summary>
    /// Get the corresponding event reference from the loaded banks. Does not wait for
    /// audio bank to be fully loaded.
    /// </summary>
    public EventReference GetEventReferenceNoAsync(string key)
    {
        if (soundEvents.TryGetValue(key, out var eventRef))
        {
            return eventRef;
        }

        Debug.LogWarning("FMODEvents: could not find event of name: " + key);
        return default;
    }

    /// <summary>
    /// Create a new instance of the given event
    /// </summary>
    public void GetEventInstance(string key, Action<EventInstance> callback)
    {
        StartCoroutine(GetEventInstanceCoroutine(key, callback));
    }
    
    public EventInstance GetEventInstanceNoAsync(string key)
    {
        if (soundEvents.TryGetValue(key, out var eventRef))
        {
            return RuntimeManager.CreateInstance(eventRef);
        }

        Debug.LogWarning("FMODEvents: could not find event of name: " + key);
        return default;
    }


    private IEnumerator LoadBanksCoroutine()
    {
        Initialized = false; // disable event referencing while loading
        soundEvents = new();

        foreach (var bank in bankNames)
        {
            RuntimeManager.LoadBank(bank, true); // force load all sample 
        }

        yield return new WaitUntil(() => (!RuntimeManager.AnySampleDataLoading() && RuntimeManager.IsInitialized));

        foreach (string name in bankNames)
        {
            string filePath = "bank:/" + name.Replace(".bank", "");
            RuntimeManager.StudioSystem.getBank(filePath, out Bank bank);

            bank.getEventList(out EventDescription[] eventDescriptions);
            foreach (EventDescription description in eventDescriptions)
            {
                description.getPath(out string eventPath);

                EventReference eventRef = RuntimeManager.PathToEventReference(eventPath);
                
                soundEvents.Add(eventPath.Substring(eventPath.LastIndexOf("/") + 1), eventRef); // the replace just makes the names a little nicer
                
                Debug.Log("Loading in to audio event: " + eventPath.Substring(eventPath.LastIndexOf("/") + 1));
            }
        }

        Initialized = true;
        loadroutine = null;
        Debug.Log("All " + soundEvents.Count + " events loaded");
    }

    private IEnumerator GetEventInstanceCoroutine(string key, Action<EventInstance> callback)
    {
        yield return new WaitUntil(() => Initialized);
        if (soundEvents.TryGetValue(key, out var eventRef))
        {
            callback?.Invoke(RuntimeManager.CreateInstance(eventRef));
        }
        else
        {
            Debug.LogWarning("FMODEvents: could not find event of name: " + key);
            callback?.Invoke(default);
        }
    }
}