using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private List<StudioEventEmitter> eventEmitters;

    private EventInstance ambience;
    private EventInstance levelMusic;

    [HideInInspector] public static AudioManager Instance { get; private set; }


    [SerializeField] bool initLevelMusic = true;
    [SerializeField] bool initAmbiance = true;

    [SerializeField, MinMaxSlider(1, 20)] private Vector2 ambianceInterval = new(1, 20);
    [SerializeField, MinMaxSlider(0, 30)] private Vector2 ambianceSpawnDist = new(0, 30);
    private float ambianceTimer;
    private bool pauseMusic;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance != null)
        {
            // this hopefully will never be seen
            UnityEngine.Debug.Log("more than one audio manager in the scene");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            eventEmitters = new List<StudioEventEmitter>();
        }
    }

    private async void Start()
    {
        if (initLevelMusic)
        {
            levelMusic = await FMODEvents.instance.initializeMusic("LevelMusic");
        }
        if (initAmbiance)
        {
            ambience = await FMODEvents.instance.initializeAmbience("testAmbience");
        }

        ambianceTimer = Random.Range(ambianceInterval.x, ambianceInterval.y);
        UnityEngine.Debug.Log($"Next random ambience in {ambianceTimer:F1} seconds");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            pauseMusic = !pauseMusic;
            UnityEngine.Debug.Log("toggle music: " + pauseMusic);

            levelMusic.setPaused(pauseMusic);
        }

        ambianceTimer -= Time.deltaTime;
        if (ambience.isValid() && ambianceTimer < 0)
        {
            Vector3 randomDir = new(Random.Range(-1, 1), Random.Range(-1, 1));
            float randomDist = Random.Range(ambianceSpawnDist.x, ambianceSpawnDist.y);
            Vector3 ambianceSpawnLoc = PlayerID.Instance.transform.position + randomDir * randomDist;
            PlayRandomAmbience(ambianceSpawnLoc);
            ambianceTimer = Random.Range(ambianceInterval.x, ambianceInterval.y);
            UnityEngine.Debug.Log($"Next random ambience in {ambianceTimer:F1} seconds");
        }
    }

    public void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambience = CreateEventInstance(ambienceEventReference);
        ambience.start();
    }

    public void InitializeMusic(EventReference musicEventReference)
    {
        levelMusic = CreateEventInstance(musicEventReference);
        levelMusic.start();
    }

    public void SetAmbienceParameter(string parameterName, float parameterValue)
    {
        ambience.setParameterByName(parameterName, parameterValue);
    }

    public void SetMusicArea(MusicArea area)
    {
        // NOTE: - string area refers to the parameter sheet in FMOD called 'area'
        //       - enum is cast to float because thats what FMOD wants I guess
        levelMusic.setParameterByName("area", (float)area);
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
    public ATTRIBUTES_3D ConfigAttributes3D(Vector3 position, Vector3 velocity, Vector3 forward, Vector3 up)
    {
        // need to add a way to orthonganize forward and up so FMOD stops getting so mad
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


    public void PlayRandomAmbience(Vector3 worldPos)
    {
        var randomList = FMODEvents.instance.getRandomSoundsList();

        if (randomList == null || randomList.Count == 0)
        {
            UnityEngine.Debug.LogWarning("Random ambience list is empty � skipping playback.");
            return;
        }

        int index = Random.Range(0, randomList.Count);
        EventReference randomEvent = randomList[index];

        RuntimeManager.PlayOneShot(randomEvent, worldPos);
        UnityEngine.Debug.Log("Played random ambience: " + randomEvent.Path);
    }

    private void OnDestroy()
    {
        if (eventEmitters != null)
        {
            foreach (StudioEventEmitter emitter in eventEmitters)
            {
                emitter.Stop();
            }
        }
    }
}
