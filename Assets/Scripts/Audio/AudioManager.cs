using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class AudioManager : Singleton<AudioManager>
{
    [Header("If we should keep ambiance on player entering this scene")]
    [SerializeField] bool initRandomAmbience;

    private List<StudioEventEmitter> eventEmitters;

    [Header("Random Ambiance Settings")]
    [SerializeField, MinMaxSlider(1, 60)] private Vector2 ambianceInterval = new(1, 20);
    [SerializeField, MinMaxSlider(0, 30)] private Vector2 ambianceSpawnDist = new(0, 30);
    private RandomAmbiancePlayer ambiancePlayer;

    protected override void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            eventEmitters = new();
        }
        else
        {
            // Pass the new manager's serializefields to the existing one
            _instance.UpdateManagerParam(initRandomAmbience, ambianceInterval, ambianceSpawnDist);
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitAmbience();
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

    #region Public Methods

    /// <summary>
    /// Play a one shot track. Will wait until banks are loaded prior to playing. Suited for
    /// tracks like music tracks.
    /// </summary>
    public void PlayOneShot(string name, Vector3 worldPos = default)
    {
        name = name.ToLower();

        StartCoroutine(PlayOneShotCoroutine(name, worldPos));
    }

    /// <summary>
    /// Play one shot track. Suited for tracks that are better voided rather than delayed, like sfx
    /// </summary>
    /// <param name="chance">Chance between 0 and 1 for the sound to play. Useful for sounds that are played frequently, like mob sfx</param>
    public void PlayOneShotNoAsync(string name, Vector3 worldPos = default, float chance = 1f)
    {
        if (Random.value > chance) return;

        name = name.ToLower();

        EventReference eventRef = FMODEvents.Instance.GetEventReferenceNoAsync(name);
        Debug.Log($"playing audio {name} {eventRef}");
        if (!eventRef.IsNull)
        {
            RuntimeManager.PlayOneShot(eventRef, worldPos);
        }
    }

    /// <summary>
    /// Play one shot track that can be stopped midway through. Suited for tracks that are better voided rather than delayed, like sfx
    /// </summary>
    public EventInstance PlayOneShotStoppableNoAsync(string name, Vector3 worldPos = default)
    {
        name = name.ToLower();

        EventReference eventRef = FMODEvents.Instance.GetEventReferenceNoAsync(name);
        if (eventRef.IsNull) return default;

        EventInstance instance = RuntimeManager.CreateInstance(eventRef);

        if (worldPos != default)
        {
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));
        }

        Debug.Log($"playing stoppable audio {name} {eventRef}");
        instance.start();

        return instance;
    }

    /// <summary>
    /// When you want a sound to play continuously until it's told to stop
    /// IMPORTANT: EventInstances must be freed via eventInstance.release after it has finished playing
    /// </summary>
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

    #endregion

    #region Helper Methods    
    /// <summary>
    /// Allows us to update fields between scene to scene
    /// </summary>
    public void UpdateManagerParam(bool initRandomAmbience, Vector2 ambianceInterval, Vector2 ambianceSpawnDist)
    {
        this.initRandomAmbience = initRandomAmbience;
        this.ambianceInterval = ambianceInterval;
        this.ambianceSpawnDist = ambianceSpawnDist;
        InitAmbience();
    }

    private void InitAmbience()
    {
        if (initRandomAmbience)
        {
            ambiancePlayer = gameObject.GetComponent<RandomAmbiancePlayer>();
            if (ambiancePlayer == null) ambiancePlayer = gameObject.AddComponent<RandomAmbiancePlayer>();
            ambiancePlayer.Init(ambianceInterval, ambianceSpawnDist, this);
        }
        else if (ambiancePlayer)
        {
            Destroy(ambiancePlayer);
        }
    }

    private IEnumerator PlayOneShotCoroutine(string name, Vector3 pos = default)
    {
        yield return new WaitUntil(() => FMODEvents.Instance.Initialized);

        name = name.ToLower();

        EventReference eventRef = FMODEvents.Instance.GetEventReferenceNoAsync(name);
        
        if (!eventRef.IsNull)
        {
            if (pos != default)
            {
                RuntimeManager.PlayOneShot(eventRef, pos);
            }
            else
            {
                RuntimeManager.PlayOneShot(eventRef);
            }
        }
    }
    #endregion
}

#region Ambience player
// Clearly I dont know how to spell ambience, I forefeit
class RandomAmbiancePlayer : MonoBehaviour
{
    AudioManager manager;
    Vector2 ambianceInterval;
    Vector2 ambianceSpawnDist;
    float ambianceTimer;

    public void Init(Vector2 interval, Vector2 spawnDist, AudioManager manager)
    {
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
#endregion