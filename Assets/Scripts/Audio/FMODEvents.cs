using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using System;
using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using FMOD.Studio;
using System.Threading.Tasks;

public class FMODEvents : SerializedMonoBehaviour
{
    public static FMODEvents instance { get; private set; }

    public List<string> bankNames = new List<string>(); // if you put a bank that doesnt exist into this list it will break the whole loop

    public static bool initialized = false;

    [OdinSerialize] public Dictionary<string, EventReference> soundEvents = new Dictionary<string, EventReference>();

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("too many instances");
            return;
        }

        instance = this;

        _ = LoadSoundEventsPostBankLoading(); // we dont care about storing this beacause its only running once
    }

    // loads all event references from all the given banks into the dictionary soundEvents using its name as the key
    private async Task LoadSoundEventsPostBankLoading()
    {
        foreach (string name in bankNames)
        {
            RuntimeManager.LoadBank(name, true); // the true forces them to all load at once
        }

        // waiting to make sure everything is loaded right
        while (RuntimeManager.AnySampleDataLoading() || !RuntimeManager.IsInitialized || !EventManager.IsInitialized)
        {
            await Task.Yield();
        }

        // The actual code
        foreach (string name in bankNames)
        {
            // Adding bank:/ cuz its needed for the path to the bank
            string filePath = "bank:/" + name.Replace(".bank", "");
            FMOD.Studio.Bank bank;

            // actually getting the bank after theyve been loaded
            RuntimeManager.StudioSystem.getBank(filePath, out bank);

            bank.getEventList(out FMOD.Studio.EventDescription[] eventDescriptions);

            foreach (FMOD.Studio.EventDescription description in eventDescriptions)
            {
                description.getPath(out string eventPath);

                EventReference eventRef = EventReference.Find(eventPath);

                soundEvents.Add(eventPath.Substring(eventPath.LastIndexOf("/") + 1), eventRef); // the replace just makes the names a little nicer
            }
        }

        initialized = true;
        Debug.Log("All " + soundEvents.Count + " events loaded");
    }

    // for repreated sounds
    public EventInstance getEventInstance(string key)
    {
        return AudioManager.Instance.CreateEventInstance(soundEvents[key]);
    }

    // sound you just wanna play once
    public void playOneShot(string key, Vector3 positon)
    {
        AudioManager.Instance.PlayOneShot(soundEvents[key], positon);
    }

    // currently all the music and ambience are initliazed inside of AudioManager so there are no examples of these being used yet
    // for making music
    public void initializeMusic(string key)
    {
        AudioManager.Instance.InitializeMusic(soundEvents[key]);
    }

    // for making ambience
    public void initializeAmbience(string key)
    {
        AudioManager.Instance.InitializeAmbience(soundEvents[key]);
    }

    // also for ambience
    public StudioEventEmitter initializeEventEmitter(string key, GameObject emitterObject)
    {
        StudioEventEmitter tempEmitter;
        tempEmitter = AudioManager.Instance.InitializeEventEmitter(soundEvents[key], emitterObject);
        tempEmitter.Play();
        return tempEmitter;
    }

    // ill make all this code work eventually trust - jay
    /*
    // the thing that will actually be called when you are trying to get an event instance
    public async Task<EventInstance> GetEventInstance(string key)
    {
        // Wait until events are ready
        Debug.Log("in getting event instance");
        while (!initialized)
        {
            Debug.Log("not initialized yet");
            await Task.Yield();
        }

        Debug.Log("trying to get event value from dict");
        // try to get the value
        if (soundEvents.TryGetValue(key, out var eventRef))
        {
            Debug.Log("returning event instance");
            return RuntimeManager.CreateInstance(eventRef);
        }

        Debug.Log("couldnt find key: " + key);
        return default;
    }
    
    
    public async Task<StudioEventEmitter> initializeEventEmitter(string key, GameObject emitterObject)
    {
        // Wait until events are ready
        while (!initialized)
        {
            await Task.Yield();
        }

        if (soundEvents.TryGetValue(key, out var eventRef))
        {
            return AudioManager.Instance.InitializeEventEmitter(eventRef, emitterObject);
        }

        Debug.Log("couldnt find key: " + key);
        return default;
    }

    public async Task<EventReference> initializeMusic(string key)
    {
        // Wait until events are ready
        while (!initialized)
        {
            await Task.Yield();
        }

        if (soundEvents.TryGetValue(key, out var eventRef))
        {
            AudioManager.Instance.InitializeMusic(eventRef);
        }

        Debug.Log("couldnt find key: " + key);
        return default;
    }

    public async Task<EventReference> initializeAmbience(string key)
    {
        // Wait until events are ready
        while (!initialized)
        {
            await Task.Yield();
        }

        if (soundEvents.TryGetValue(key, out var eventRef))
        {
            AudioManager.Instance.InitializeAmbience(eventRef);
        }

        Debug.Log("couldnt find key: " + key);
        return default;
    }
    */
}
