using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MusicManager : Singleton<MusicManager>
{
    [Header("If we should keep level music on player entering this scene")]
    [SerializeField] bool initLevelMusic;

    private List<StudioEventEmitter> eventEmitters;

    // the currently playing track
    public EventInstance curTrack;

    public string initMusicKey = "Area1ForestAmbiance";

    [ShowInInspector, ReadOnly] public Dictionary<string, EventInstance> musicEventInstances = new();
    
    private Coroutine crossFadeRoutine;
    private Coroutine activeFadeRoutine;

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
            _instance.UpdateManagerParam(initLevelMusic);
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitMusicOnStart();
    }

    protected override void OnDestroy()
    {
        if (eventEmitters != null)
        {
            foreach (StudioEventEmitter emitter in eventEmitters)
            {
                emitter.Stop();
            }
        }
        base.OnDestroy();
    }

    #region Methods

    /// <summary>
    /// Change level music to a different region's
    /// </summary>
    public void SetMusicArea(MusicArea area)
    {
        // TODO change is abrupt rn. Need to update to use Multi-instrument and crossfade
        // between tracks in FMOD : )
        curTrack.setParameterByName("area", (int)area);
        Debug.Log("setting music area to " + area);
    }

    /// <summary>
    /// Allows us to update fields between scene to scene
    /// </summary>
    public void UpdateManagerParam(bool initLevelMusic)
    {
        if (this.initLevelMusic != initLevelMusic)
        {
            this.initLevelMusic = initLevelMusic;
            InitMusicOnStart();
        }
    }

    private void InitMusicOnStart()
    {
        StartCoroutine(InitMusicOnStartCoroutine());
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

    /// <summary>
    /// Crossfades from the curTrack to a track that you give the key of over a given duration
    /// </summary>
    public void CrossFadeMusic(string toKey, float duration)
    {
        if (crossFadeRoutine != null)
        {
            StopCoroutine(crossFadeRoutine);
        }

        crossFadeRoutine = StartCoroutine(MusicCrossFade(toKey, duration));
    }

    private IEnumerator MusicCrossFade(string toKey, float duration)
    {
        toKey = toKey.ToLower();

        EventInstance to = InitalizeMusicNotStart(toKey);

        // we are always crossfading from the current track to something
        EventInstance from = curTrack;

        float curTime = 0f;

        // if to isnt already playing play it
        to.getPlaybackState(out PLAYBACK_STATE state);
        if (state != PLAYBACK_STATE.PLAYING)
        {
            to.start();
        }

        // resetting vals to make sure it transfers right
        from.setVolume(1f);
        to.setVolume(0f);

        while (curTime < duration)
        {
            curTime += Time.deltaTime; // because its framebased it could cause issues but that fine for now
            float t = curTime / duration;

            float smoothedT = Mathf.SmoothStep(0f, 1f, t);

            // 3. Equal Power Crossfade Math
            // Fade In: sin(t * pi/2)
            // Fade Out: cos(t * pi/2)
            float fadeInVol = Mathf.Sin(smoothedT * Mathf.PI * 0.5f);
            float fadeOutVol = Mathf.Cos(smoothedT * Mathf.PI * 0.5f);

            to.setVolume(fadeInVol);
            from.setVolume(fadeOutVol);

            yield return null; // wait for a frame in between loop runs
        }

        from.setVolume(0f);
        to.setVolume(1f);

        curTrack = to;

        from.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    // also checks to see if that music event is already in the dict
    private EventInstance InitalizeMusicNotStart(string key)
    {
        key = key.ToLower();

        if (musicEventInstances.TryGetValue(key, out var eventInstance))
        {
            EventInstance tempInstance = eventInstance;
            return tempInstance;
        }
        else
        {
            EventInstance tempInstance = FMODEvents.Instance.GetEventInstanceNoAsync(key);

            musicEventInstances.Add(key, tempInstance);

            return tempInstance;
        }
    }

    private IEnumerator InitMusicOnStartCoroutine()
    {
        yield return new WaitUntil(() => FMODEvents.Instance.Initialized);

        if (initLevelMusic)
        {
            if (musicEventInstances.TryGetValue(initMusicKey, out var eventInstance))
            {
                curTrack = eventInstance;
                curTrack.start();
            }
            else
            {
                FMODEvents.Instance.GetEventInstance(initMusicKey, instance => {
                    curTrack = instance;
                    musicEventInstances.Add(initMusicKey, curTrack);
                    curTrack.start();
                });

            }
        }
        else
        {
            if (curTrack.isValid())
            {
                curTrack.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }

    /// <summary>
    /// Fades in/out the combat part of the curTrack
    /// </summary>
    public void ToggleComabatVolume()
    {
        if (activeFadeRoutine != null)
        {
            StopCoroutine(activeFadeRoutine);
        }

        activeFadeRoutine = StartCoroutine(FadeCombatVolume());
    }

    private IEnumerator FadeCombatVolume()
    {
        RuntimeManager.StudioSystem.getParameterByName("Combat Track Volume", out float vol);

        Debug.Log("vol is: " + vol);

        if (vol < 0.5f)
        {
            float duration = 2f;

            float currentT = Mathf.Asin(vol) / (Mathf.PI * 0.5f);
            float curTime = currentT * duration;

            while (curTime < duration)
            {
                curTime += Time.deltaTime;
                float t = Mathf.Clamp01(curTime / duration);
                float smoothedT = Mathf.SmoothStep(0f, 1f, t);

                float fadeInVol = Mathf.Sin(smoothedT * Mathf.PI * 0.5f);
                RuntimeManager.StudioSystem.setParameterByName("Combat Track Volume", fadeInVol);

                yield return null;
            }

            RuntimeManager.StudioSystem.setParameterByName("Combat Track Volume", 1);
        }
        else
        {
            float duration = 9f;

            float currentT = Mathf.Acos(vol) / (Mathf.PI * 0.5f);
            float curTime = currentT * duration;

            while (curTime < duration)
            {
                curTime += Time.deltaTime;
                float t = Mathf.Clamp01(curTime / duration);
                float smoothedT = Mathf.SmoothStep(0f, 1f, t);

                float fadeOutVol = Mathf.Cos(smoothedT * Mathf.PI * 0.5f);
                RuntimeManager.StudioSystem.setParameterByName("Combat Track Volume", fadeOutVol);

                yield return null;
            }

            RuntimeManager.StudioSystem.setParameterByName("Combat Track Volume", 0);
        }

        activeFadeRoutine = null;
    }

    #endregion
}
