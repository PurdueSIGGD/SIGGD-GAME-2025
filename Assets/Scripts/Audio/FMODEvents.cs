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

        Debug.LogWarning("FMODEvents: could not find event of name: " + key);
        callback?.Invoke(default);
    }


























    //private void Awake()
    //{
    //    if (instance != null)
    //    {
    //        Debug.Log("too many instances");
    //        return;
    //    }

    //    instance = this;

    //    _ = LoadSoundEventsPostBankLoading(); // we dont care about storing this beacause its only running once
    //    _ = LoadRandomAmbienceList();
    //}

    //// loads all event references from all the given banks into the dictionary soundEvents using its name as the key
    //private async Task LoadSoundEventsPostBankLoading()
    //{
    //    foreach (string name in bankNames)
    //    {
    //        RuntimeManager.LoadBank(name, true); // the true forces them to all load at once
    //    }

    //    // waiting to make sure everything is loaded right
    //    //while (RuntimeManager.AnySampleDataLoading() || !RuntimeManager.IsInitialized || !EventManager.IsInitialized)
    //    //{
    //    //    await Task.Yield();
    //    //}
    //    // ^^^^^ COMMENTED OUT FOR BUILD
    //    while (RuntimeManager.AnySampleDataLoading() || !RuntimeManager.IsInitialized)
    //    {
    //        await Task.Yield();
    //    }


    //}

    //private async Task LoadRandomAmbienceList()
    //{

    //    RuntimeManager.LoadBank("RandomAmbience", true);

    //    // waiting to make sure everything is loaded right
    //    //while (RuntimeManager.AnySampleDataLoading() || !RuntimeManager.IsInitialized || !EventManager.IsInitialized)
    //    //{
    //    //    await Task.Yield();
    //    //}
    //    // ^^^^^ COMMENTED OUT FOR BUILD
    //    while (RuntimeManager.AnySampleDataLoading() || !RuntimeManager.IsInitialized)
    //    {
    //        await Task.Yield();
    //    }

    //    // The actual code
    //    // Adding bank:/ cuz its needed for the path to the bank
    //    string filePath = "bank:/RandomAmbience";
    //    FMOD.Studio.Bank bank;

    //    // actually getting the bank after theyve been loaded
    //    RuntimeManager.StudioSystem.getBank(filePath, out bank);

    //    bank.getEventList(out FMOD.Studio.EventDescription[] eventDescriptions);

    //    foreach (FMOD.Studio.EventDescription description in eventDescriptions)
    //    {
    //        description.getPath(out string eventPath);

    //        EventReference eventRef = FMODUnity.RuntimeManager.PathToEventReference(eventPath);

    //        randomSounds.Add(eventRef); // the replace just makes the names a little nicer
    //    }

    //    initialized = true;
    //    Debug.Log("All " + randomSounds.Count + " events loaded");
    //}

    //// something for ambience i dont understand it but i hope it works
    //public async Task<StudioEventEmitter> initializeEventEmitter(string key, GameObject emitterObject)
    //{
    //    // Wait until events are ready
    //    while (!Initialized)
    //    {
    //        await Task.Yield();
    //    }

    //    if (soundEvents.TryGetValue(key, out var eventRef))
    //    {
    //        return AudioManager.Instance.InitializeEventEmitter(eventRef, emitterObject);
    //    }

    //    Debug.Log("couldnt find key: " + key);
    //    return default;
    //}

    //public async Task<EventInstance> initializeMusic(string key)
    //{
    //    // Wait until events are ready
    //    while (!Initialized)
    //    {
    //        await Task.Yield();
    //    }

    //    if (soundEvents.TryGetValue(key, out var eventRef))
    //    {
    //        AudioManager.Instance.InitializeMusic(eventRef);
    //    }

    //    Debug.Log("couldnt find key: " + key);
    //    return default;
    //}

    //public async Task<EventInstance> initializeAmbience(string key)
    //{
    //    // Wait until events are ready
    //    while (!Initialized)
    //    {
    //        await Task.Yield();
    //    }

    //    if (soundEvents.TryGetValue(key, out var eventRef))
    //    {
    //        AudioManager.Instance.InitializeAmbience(eventRef);
    //    }

    //    Debug.Log("couldnt find key: " + key);
    //    return default;
    //}
}