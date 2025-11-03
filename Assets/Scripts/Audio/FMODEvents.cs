using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
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

    public List<EventReference> randomSounds = new List<EventReference>();

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("too many instances");
            return;
        }

        instance = this;

        _ = LoadSoundEventsPostBankLoading(); // we dont care about storing this beacause its only running once
        _ = LoadRandomAmbienceList();
    }

    // loads all event references from all the given banks into the dictionary soundEvents using its name as the key
    private async Task LoadSoundEventsPostBankLoading()
    {
        foreach (string name in bankNames)
        {
            RuntimeManager.LoadBank(name, true); // the true forces them to all load at once
        }

        // waiting to make sure everything is loaded right
        //while (RuntimeManager.AnySampleDataLoading() || !RuntimeManager.IsInitialized || !EventManager.IsInitialized)
        //{
        //    await Task.Yield();
        //}
        // ^^^^^ COMMENTED OUT FOR BUILD
        while (RuntimeManager.AnySampleDataLoading() || !RuntimeManager.IsInitialized)
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

    private async Task LoadRandomAmbienceList()
    {
        
        RuntimeManager.LoadBank("RandomAmbience", true);

        // waiting to make sure everything is loaded right
        //while (RuntimeManager.AnySampleDataLoading() || !RuntimeManager.IsInitialized || !EventManager.IsInitialized)
        //{
        //    await Task.Yield();
        //}
        // ^^^^^ COMMENTED OUT FOR BUILD
        while (RuntimeManager.AnySampleDataLoading() || !RuntimeManager.IsInitialized)
        {
            await Task.Yield();
        }

        // The actual code
        // Adding bank:/ cuz its needed for the path to the bank
        string filePath = "bank:/RandomAmbience";
        FMOD.Studio.Bank bank;

        // actually getting the bank after theyve been loaded
        RuntimeManager.StudioSystem.getBank(filePath, out bank);

        bank.getEventList(out FMOD.Studio.EventDescription[] eventDescriptions);

        foreach (FMOD.Studio.EventDescription description in eventDescriptions)
        {
            description.getPath(out string eventPath);

            EventReference eventRef = EventReference.Find(eventPath);

            randomSounds.Add(eventRef); // the replace just makes the names a little nicer
        }

        initialized = true;
        Debug.Log("All " + randomSounds.Count + " events loaded");
    }

    public List<EventReference> getRandomSoundsList()
    {
        return randomSounds;
    }

    public void playOneShot(string key, Vector3 position)
    {
        AudioManager.Instance.PlayOneShot(soundEvents[key], position);
    }

    // gets you an event instance based on the key you give it
    public async Task<EventInstance> getEventInstance(string key)
    {
        // Wait until events are ready
        while (!initialized)
        {
            await Task.Yield();
        }

        // tries to get the value
        if (soundEvents.TryGetValue(key, out var eventRef))
        {
            return RuntimeManager.CreateInstance(eventRef);
        }

        // if it doesnt it logs what it couldnt find
        Debug.Log("couldnt find key: " + key);
        return default;
    }

    public EventInstance getEventInstanceNOASYNC(string key)
    {
        // tries to get the value
        if (soundEvents.TryGetValue(key, out var eventRef))
        {
            return RuntimeManager.CreateInstance(eventRef);
        }

        // if it doesnt it logs what it couldnt find
        Debug.Log("couldnt find key: " + key);
        return default;
    }

    // something for ambience i dont understand it but i hope it works
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

    public async Task<EventInstance> initializeMusic(string key)
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

    public async Task<EventInstance> initializeAmbience(string key)
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
}