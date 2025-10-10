using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using System;

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

    [field: SerializeField] public List<EventReference> sounds { get; private set; }

    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            // another message that shouldnt be seen
            Debug.Log("too many instances");
        }

        instance = this;
    }
}
