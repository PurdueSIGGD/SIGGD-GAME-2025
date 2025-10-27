using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;

public class AudioManager : MonoBehaviour
{
    private List<StudioEventEmitter> eventEmitters;

    private EventInstance ambience;
    private EventInstance music;

    public static AudioManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null)
        {
            // this hopefully will never be seen
            UnityEngine.Debug.Log("more than one audio manager in the scene");
        }
        Instance = this;
        
        eventEmitters = new List<StudioEventEmitter>();
    }

    private async void Start()
    {
        music = await FMODEvents.instance.initializeMusic("LevelMusic");
        ambience = await FMODEvents.instance.initializeAmbience("testAmbience");
    }

    public void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambience = CreateEventInstance(ambienceEventReference);
        ambience.start();
    }

    public void InitializeMusic(EventReference musicEventReference)
    {
        music = CreateEventInstance(musicEventReference);
        music.start();
    }

    public void SetAmbienceParameter(string parameterName, float parameterValue)
    {
        ambience.setParameterByName(parameterName, parameterValue);
    }

    public void SetMusicArea(MusicArea area)
    {
        // NOTE: - string area refers to the parameter sheet in FMOD called 'area'
        //       - enum is cast to float because thats what FMOD wants I guess
        music.setParameterByName("area", (float)area);
        UnityEngine.Debug.Log("setting music area to " + area);
    }

    // when you just want to play a sound once on a trigger
    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    // when you want a sound to play continuously until it's told to stop
    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        return eventInstance;
    }

    // NOTE: 3d attributes need to be set in order to play instances in 3d
    public ATTRIBUTES_3D configAttributes3D(Vector3 position, Vector3 velocity, Vector3 forward, Vector3 up)
    {
        VECTOR pos = new VECTOR { x = position.x, y = position.y, z = position.z };
        VECTOR vel = new VECTOR { x = velocity.x, y = velocity.y, z = velocity.z };
        VECTOR forw = new VECTOR { x = forward.x, y = forward.y, z = forward.z };
        VECTOR upAttr = new VECTOR { x = up.x, y = up.y, z = up.z };
        return new ATTRIBUTES_3D { position = pos, velocity = vel, forward = forw, up = upAttr };
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObj)
    {
        StudioEventEmitter emitter = emitterGameObj.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }


    private bool pauseMusic = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            pauseMusic = !pauseMusic;
            UnityEngine.Debug.Log("toggle music: " + pauseMusic);

            music.setPaused(pauseMusic);
        }

    }

    private void OnDestroy()
    {
        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }
}