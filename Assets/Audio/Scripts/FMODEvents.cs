using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using System;
using System.Collections;

public class FMODEvents : MonoBehaviour
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


    public static FMODEvents instance { get; private set; }

    public List<string> bankNames = new List<string>();

    public Dictionary<string, EventReference> soundEvents = new Dictionary<string, EventReference>();

    private void Awake()
    {
        if (instance != null)
        {
            // another message that shouldnt be seen
            Debug.Log("too many instances");
        }

        instance = this;

        foreach (string name in bankNames)
        {
            RuntimeManager.LoadBank(name, true); // the true forces them to all load at once
        }

        StartCoroutine(loadEventsPostBankLoading());
    }

    private IEnumerator loadEventsPostBankLoading()
    {
        yield return null; // it loads the banks early if you remove this (i have no clue as to why)

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
