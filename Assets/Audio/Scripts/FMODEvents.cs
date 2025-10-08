using UnityEngine;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.Serialization;

public class FMODEvents : SerializedMonoBehaviour
{
    public static FMODEvents instance { get; private set; }

    [OdinSerialize] public Dictionary<string, EventReference> soundEvents = new Dictionary<string, EventReference>();

    public List<string> bankNames = new List<string>(); // add bank names to this list

    private void Awake()
    {
        if (instance != null)
        {
            // another message that shouldnt be seen
            Debug.Log("too many instances");
        }

        instance = this;
    }

    private void Start()
    {
        // there are no error checks because if you dont input something wrong it wont output anything wrong
        foreach (string name in bankNames)
        {
            RuntimeManager.LoadBank(name, true); // the true forces them to all load at once
        }

        StartCoroutine(loadEventsPostBankLoading());
    }

    // the name kinda says it all this is made to wait until all of FMOD is done loading before trying to make event references
    private IEnumerator loadEventsPostBankLoading()
    {
        yield return null; // it loads the banks early if you remove this (i have no F*cking clue as to why)
        // the waiting code
        while (RuntimeManager.AnySampleDataLoading())
        {
            yield return null;
        }

        // The actual code
        foreach (string name in bankNames)
        {
            // removing .bank from the inital string given and adding bank:/ cuz its needed for the path to the bank
            string filePath = "bank:/" + name.Replace(".bank", "");
            FMOD.Studio.Bank bank;

            // actually getting the bank after theyve been loaded
            RuntimeManager.StudioSystem.getBank(filePath, out bank);

            bank.getEventList(out FMOD.Studio.EventDescription[] eventDescriptions);

            foreach (FMOD.Studio.EventDescription description in eventDescriptions)
            {
                description.getPath(out string eventPath);

                EventReference eventRef = EventReference.Find(eventPath);
                Debug.Log("eventPath is: " + eventPath);
                Debug.Log("eventRef is: " + eventRef);

                soundEvents.Add(eventPath.Replace("event:/", ""), eventRef); // the replace just makes the names a little nicer
            }
        }
            

        Debug.Log("All " + soundEvents.Count + " events loaded");
    }
}

/*
using System.Collections.Generic;
using FMODUnity; // Make sure you have this using statement

public class YourClassName // Replace with the name of your actual class
{
    // Your dictionary declaration
    public Dictionary<string, EventReference> soundEvents = new Dictionary<string, EventReference>();

    // Assuming 'bankNames' is a list/array of strings (e.g., {"Master.bank", "SFX.bank"})
    public string[] bankNames;

    private void Start()
    {
        foreach (string name in bankNames)
        {
            // The FMOD RuntimeManager usually handles the file extension when loading
            // and the 'bank:/' path for getting the bank.

            RuntimeManager.LoadBank(name);

            // It's often safer to get the bank once the load is complete or handle
            // the async nature of bank loading, but for synchronous loading this is fine.
            string forPathName = name.Replace(".bank", "");

            // Get the loaded bank.
            FMOD.Studio.Bank bank;
            FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;

            // Check for success before proceeding
            if (studioSystem.getBank("bank:/" + forPathName, out bank) == FMOD.RESULT.OK)
            {
                // Get the list of all event descriptions in the bank.
                if (bank.getEventList(out FMOD.Studio.EventDescription[] events) == FMOD.RESULT.OK)
                {
                    foreach (FMOD.Studio.EventDescription eventDescription in events)
                    {
                        // Get the path of the event, which will be the key for your dictionary.
                        // The path will look something like "event:/SFX/Explosion".
                        if (eventDescription.getPath(out string eventPath) == FMOD.RESULT.OK)
                        {
                            // Create an EventReference from the path. This is the FMOD Unity wrapper type.
                            EventReference eventRef = EventReference.FullPath(eventPath);

                            // Add the event path (key) and the EventReference (value) to the dictionary.
                            // Use TryAdd for safety in case an event exists in multiple banks (though 
                            // this shouldn't happen with well-managed FMOD projects).
                            soundEvents.TryAdd(eventPath, eventRef);

                            // Alternative to TryAdd if you're on an older C# version (pre-C# 7.0):
                            // if (!soundEvents.ContainsKey(eventPath))
                            // {
                            //     soundEvents.Add(eventPath, eventRef);
                            // }
                        }
                        // Handle error if getPath fails if needed
                    }
                }
                // Handle error if getEventList fails if needed
            }
            // Handle error if getBank fails if needed
        }

        // Optional: Check how many events were loaded
        // UnityEngine.Debug.Log($"Loaded {soundEvents.Count} sound events.");
    }
}
*/