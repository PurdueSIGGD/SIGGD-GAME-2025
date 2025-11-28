using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] bool initLevelMusic;
    [SerializeField] bool initRandomAmbience;

    private List<StudioEventEmitter> eventEmitters;
    private EventInstance levelMusic;
    private bool pauseMusic;

    [Header("Random Ambiance Settings")]
    [SerializeField, MinMaxSlider(1, 20)] private Vector2 ambianceInterval = new(1, 20);
    [SerializeField, MinMaxSlider(0, 30)] private Vector2 ambianceSpawnDist = new(0, 30);
    private RandomAmbiancePlayer ambiancePlayer;

    protected override void Awake()
    {
        base.Awake();
        eventEmitters = new();
    }

    private void Start()
    {
        if (initLevelMusic)
        {
            FMODEvents.Instance.GetEventInstance("Level Music", instance => { 
                levelMusic = instance;
                levelMusic.start();
            });
        }
        if (initRandomAmbience)
        {
            ambiancePlayer = gameObject.AddComponent<RandomAmbiancePlayer>();
            ambiancePlayer.Init(ambianceInterval, ambianceSpawnDist, this);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.M))
        {
            pauseMusic = !pauseMusic;
            Debug.Log("toggle music: " + pauseMusic);

            levelMusic.setPaused(pauseMusic);
        }
#endif
    }

    /// <summary>
    /// Change level music to a different region's
    /// </summary>
    public void SetMusicArea(MusicArea area)
    {
        levelMusic.setParameterByName("area", (int)area);
        Debug.Log("setting music area to " + area);
    }

    /// <summary>
    /// Play a one shot track. Will wait until banks are loaded prior to playing. Suited for
    /// tracks like music tracks.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="worldPos"></param>
    public void PlayOneShot(string name, Vector3 worldPos = default)
    {
        StartCoroutine(PlayOneShotCoroutine(name, worldPos));
    }

    /// <summary>
    /// Play one shot track. Suited for tracks that are better voided rather than delayed, like sfx
    /// </summary>
    /// <param name="name"></param>
    /// <param name="worldPos"></param>
    public void PlayOneShotNoAsync(string name, Vector3 worldPos = default)
    {
        EventReference eventRef = FMODEvents.Instance.GetEventReferenceNoAsync(name);
        if (!eventRef.IsNull)
        {
            RuntimeManager.PlayOneShot(eventRef, worldPos);
        }
    }

    /// <summary>
    /// When you want a sound to play continuously until it's told to stop
    /// </summary>
    /// <param name="eventReference"></param>
    /// <returns></returns>
    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        return eventInstance;
    }

    /// <summary>
    /// Create a 3d attribute to be used by an event instance to play sound in 3d
    /// </summary>
    public ATTRIBUTES_3D ConfigAttributes3D(Vector3 position, Vector3 velocity, Vector3 forward, Vector3 up)
    {
        // TODO need to add a way to orthonganize forward and up so FMOD stops getting so mad
        VECTOR pos = new VECTOR { x = position.x, y = position.y, z = position.z };
        VECTOR vel = new VECTOR { x = velocity.x, y = velocity.y, z = velocity.z };
        VECTOR forw = new VECTOR { x = forward.x, y = forward.y, z = forward.z };
        VECTOR upAttr = new VECTOR { x = up.x, y = up.y, z = up.z };
        return new ATTRIBUTES_3D { position = pos, velocity = vel, forward = forw, up = upAttr };
    }

    /// <summary>
    /// Register an event emitter
    /// </summary>
    /// <param name="eventReference">the sound to be played on trigger</param>
    /// <param name="emitterGameObj">the event emitter's parent object</param>
    /// <returns>Ref to the registered event emitter</returns>
    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObj)
    {
        StudioEventEmitter emitter = emitterGameObj.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    protected override void OnDestroy()
    {
        if (ambiancePlayer != null)
        {
            Destroy(ambiancePlayer);
        }
        if (eventEmitters != null)
        {
            foreach (StudioEventEmitter emitter in eventEmitters)
            {
                emitter.Stop();
            }
        }
        base.OnDestroy();
    }

    private IEnumerator PlayOneShotCoroutine(string name, Vector3 pos = default)
    {
        yield return new WaitUntil(() => FMODEvents.Instance.Initialized);
        EventReference eventRef = FMODEvents.Instance.GetEventReferenceNoAsync(name);
        if (!eventRef.IsNull)
        {
            RuntimeManager.PlayOneShot(eventRef, pos);
        }
    }
}

[Serializable]
class RandomAmbiancePlayer : MonoBehaviour
{
    AudioManager manager;
    Vector2 ambianceInterval;
    Vector2 ambianceSpawnDist;
    float ambianceTimer;

    public void Init(Vector2 interval, Vector2 spawnDist, AudioManager manager) {
        this.manager = manager;
        ambianceInterval = interval;
        ambianceSpawnDist = spawnDist;

        ambianceTimer = Random.Range(ambianceInterval.x, ambianceInterval.y);
        Debug.Log($"Next random ambience in {ambianceTimer:F1} seconds");
    }

    void Update()
    {
        if (manager)
        {
            ambianceTimer -= Time.deltaTime;
            if (ambianceTimer < 0)
            {
                Vector3 randomDir = new(Random.Range(-1, 1), Random.Range(-1, 1));
                float randomDist = Random.Range(ambianceSpawnDist.x, ambianceSpawnDist.y);
                Vector3 ambianceSpawnLoc = randomDir * randomDist;
                if (PlayerID.Instance) ambianceSpawnLoc += PlayerID.Instance.transform.position;

                PlayRandomAmbience(ambianceSpawnLoc);
                ambianceTimer = Random.Range(ambianceInterval.x, ambianceInterval.y);
                Debug.Log($"Next random ambience in {ambianceTimer:F1} seconds");
            }
        }
    }

    public void PlayRandomAmbience(Vector3 worldPos)
    {
        manager.PlayOneShot("Random Ambience", worldPos);
    }
}