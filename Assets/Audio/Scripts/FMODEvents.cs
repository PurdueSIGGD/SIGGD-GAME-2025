using UnityEngine;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.Serialization;


[Serializable]
public struct eventRefs
{
    public string name;
    public EventReference reference;
}
public class FMODEvents : SerializedMonoBehaviour
{
    public static FMODEvents instance { get; private set; }

    public Dictionary<string, EventReference> soundEvents = new Dictionary<string, EventReference>();

    public List<eventRefs> eventRefsList = new List<eventRefs>();

    private List<string> bankNames = new List<string>(); // add bank names to this list

    private void Awake()
    {
        if (instance != null)
        {
            // another message that shouldnt be seen
            Debug.Log("too many instances");
        }

        instance = this;

        // loop that adds all values from the list into the dictionary
        foreach(var eventRefs in eventRefsList)
        {
            soundEvents.Add(eventRefs.name, eventRefs.reference);
        }
    }

    private void Start()
    {
        // there are no error checks because if you dont input something wrong it wont output anything wrong
        /*
        foreach (string name in bankNames)
        {
            RuntimeManager.LoadBank(name, true); // the true forces them to all load at once
        }
        */
        //StartCoroutine(loadEventsPostBankLoading());
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