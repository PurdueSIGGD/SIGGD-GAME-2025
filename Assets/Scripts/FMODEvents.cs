using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using UnityEditor;
using Sirenix.Serialization;

public class FMODEvents : SerializedMonoBehaviour
{
    [field: Header("Player SFX")]
    [OdinSerialize] public EventReference enemyDeath { get; private set; }

    [field: Header("Footsteps")]
    [OdinSerialize] public EventReference footsteps { get; private set; }

    [field: Header("Background Music")]
    [OdinSerialize] public EventReference music { get; private set; }
    
    public static FMODEvents instance { get; private set; }

    //[OdinSerialize] public static StringEventReferenceDictionary soundEvents = new StringEventReferenceDictionary();

    [OdinSerialize] public Dictionary<string, EventReference> soundEvents = new Dictionary<string, EventReference>();

    private void Awake()
    {
        if (instance != null)
        {
            // another message that shouldnt be seen
            Debug.Log("too many instances");
        }

        instance = this;

        soundEvents.Add("Footsteps", footsteps);
    }
}
