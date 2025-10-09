using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(StudioEventEmitter))]

public class Server : MonoBehaviour
{
    private StudioEventEmitter emitter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.serverNoise, this.gameObject);
        emitter.Play();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        emitter.Stop();
    }
}
