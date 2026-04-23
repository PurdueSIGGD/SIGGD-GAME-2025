using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class MusicManager : Singleton<MusicManager>
{
    [Header("If you want the initialized music method to run (should only be true in the first scene that opens in the game)")]
    [SerializeField] bool initLevelMusic;

    private List<StudioEventEmitter> eventEmitters;

    // the currently playing track
    public EventInstance curTrack;

    [Header("This is the music you want playing when the game starts not when this scene is loaded")]
    [SerializeField] private string initMusicKey;

    [ShowInInspector, ReadOnly] public Dictionary<string, EventInstance> musicEventInstances = new();
    
    private Coroutine crossFadeRoutine;
    private Coroutine activeFadeRoutine;
    private Coroutine pauseMusicRoutine;
    private Coroutine playMusicRoutine;

    public enum MusicCycleState { Playing, FadingOut, FadingIn, NotPlaying, Combat, Stopped, ApexLurk}

    [SerializeField] private MusicCycleState curMusicState = MusicCycleState.NotPlaying;

    // set this to true whenever the player gets into combat
    private bool inCombat = false;


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
        foreach (var instance in musicEventInstances.Values)
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }
        base.OnDestroy();
    }

    #region Scene Management and Game State Management
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
           
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop the current track so we don't have overlapping music
        if (curTrack.isValid())
        {
            curTrack.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            curMusicState = MusicCycleState.Stopped;
        }
    }

    #endregion

    #region Public Methods

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

    /// <summary>
    /// Overrides curTrack with the key you passed in
    /// </summary>
    public void SetCurTrack(string key)
    {
        Debug.Log("Cur Track is " + key);
        curTrack = InitalizeMusicNotStart(key);
    }

    /// <summary>
    /// Fades out the current track if allowFade is true, immediatly stops it if allowFade is false
    /// </summary>
    public void PauseMusic(bool allowFade)
    {
        curTrack.getPaused(out bool paused);
        if (paused == true)
        {
            curMusicState = MusicCycleState.NotPlaying;
            return;
        }

        Debug.Log("Pause Music");

        if (allowFade == false)
        {
            curTrack.setPaused(true);
            curMusicState = MusicCycleState.NotPlaying;
            return;
        }

        if (playMusicRoutine != null)
        {
            StopCoroutine(playMusicRoutine);
            playMusicRoutine = null;
        }
        // if we are already pausing the music no need to do it again
        if (pauseMusicRoutine == null)
        {
            pauseMusicRoutine = StartCoroutine(fadeOutMusic(5f));
        }
    }

    /// <summary>
    /// Fades in the current track if allowFade is true, starts it immediatly if allowFade is false
    /// </summary>
    public void PlayMusic(bool allowFade)
    {
        Debug.Log("in play music");
        curTrack.getPlaybackState(out PLAYBACK_STATE state);
        if (state != PLAYBACK_STATE.PLAYING)
        {
            Debug.Log("start cuz stopped");
            curTrack.start();
            curMusicState = MusicCycleState.Playing;
        }
        else
        {
            curTrack.getPaused(out bool paused);
            if (paused == false)
            {
                Debug.Log("return cuz not paused");
                return;
            }
        }

        Debug.Log("PlayMusic");

        if (allowFade == false)
        {
            curTrack.setPaused(false);
            curMusicState = MusicCycleState.Playing;
            return;
        }

        if (pauseMusicRoutine != null)
        {
            StopCoroutine(pauseMusicRoutine);
            pauseMusicRoutine = null;
        }
        if (playMusicRoutine == null)
        {
            StartCoroutine(fadeInMusic(5f));
        }
    }
    /// <summary>
    /// Crossfades from the curTrack to a track that you give the key of over a given duration
    /// </summary>
    public void CrossFadeMusic(string toKey, float duration)
    {
        Debug.Log("crossfading music");
        if (crossFadeRoutine != null)
        {
            StopCoroutine(crossFadeRoutine);
        }

        crossFadeRoutine = StartCoroutine(MusicCrossFade(toKey, duration));
    }

    /// <summary>
    /// Fades in/out the combat part of the curTrack
    /// </summary>
    public void ToggleComabatVolume()
    {
        Debug.Log("Combat volume toggled");
        if (activeFadeRoutine != null)
        {
            StopCoroutine(activeFadeRoutine);
        }

        activeFadeRoutine = StartCoroutine(FadeCombatVolume());
    }

    #endregion

    #region Private Methods
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

    private IEnumerator FadeCombatVolume()
    {
        RuntimeManager.StudioSystem.getParameterByName("Combat Track Volume", out float combatVol);

        if (combatVol < 0.5f)
        {
            curMusicState = MusicCycleState.Combat;
            // fade regular track volume to full volume when fading combat volume on
            curTrack.getVolume(out float vol);
            
            float duration = 2f;

            float currentT = Mathf.Asin(combatVol) / (Mathf.PI * 0.5f);
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
            curMusicState = MusicCycleState.Playing;
            float duration = 9f;

            float currentT = Mathf.Acos(combatVol) / (Mathf.PI * 0.5f);
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
        from.release();

        crossFadeRoutine = null;
    }

    // also checks to see if that music event is already in the dict
    private EventInstance InitalizeMusicNotStart(string key)
    {
        key = key.ToLower();

        curMusicState = MusicCycleState.NotPlaying;

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
        yield return null;

        if (initLevelMusic)
        {
            if (musicEventInstances.TryGetValue(initMusicKey, out var eventInstance))
            {
                curTrack = eventInstance;
                curTrack.start();
                curMusicState = MusicCycleState.Playing;
                Debug.Log("cur track playing");
            }
            else
            {
                Debug.LogError("Initiliazed Track Doesnt Exist");
            }
        }
    }
    private IEnumerator fadeInMusic(float duration)
    {
        curTrack.setPaused(false);

        curTrack.getVolume(out float vol);

        float curTime = Mathf.Asin(vol) / (Mathf.PI * 0.5f);

        while (curTime < duration)
        {
            curMusicState = MusicCycleState.FadingIn;
            curTime += Time.deltaTime; // because its framebased it could cause issues but that fine for now
            float t = curTime / duration;

            float smoothedT = Mathf.SmoothStep(0f, 1f, t);

            // Fade In: sin(t * pi/2)
            float fadeInVol = Mathf.Sin(smoothedT * Mathf.PI * 0.5f);

            curTrack.setVolume(fadeInVol);

            yield return null; // wait for a frame in between loop runs
        }

        curTrack.setVolume(1f);

        playMusicRoutine = null;

        curMusicState = MusicCycleState.Playing;
    }
    private IEnumerator fadeOutMusic(float duration)
    {
        curTrack.getVolume(out float vol);

        float curTime = Mathf.Acos(vol) / (Mathf.PI * 0.5f);

        while (curTime < duration)
        {
            curMusicState = MusicCycleState.FadingOut;
            curTime += Time.deltaTime; // because its framebased it could cause issues but that fine for now
            float t = curTime / duration;

            float smoothedT = Mathf.SmoothStep(0f, 1f, t);

            // Fade Out: cos(t * pi/2)
            float fadeOutVol = Mathf.Cos(smoothedT * Mathf.PI * 0.5f);

            curTrack.setVolume(fadeOutVol);

            yield return null; // wait for a frame in between loop runs
        }

        curTrack.setVolume(0f);

        curTrack.setPaused(true);

        pauseMusicRoutine = null;

        curMusicState = MusicCycleState.NotPlaying;
    }

    /// <summary>
    /// This corutine smoothly changes track volume over a given amount of seconds
    /// </summary>
    private IEnumerator FadeTrackVolume(float targetVolume, float duration)
    {
        curTrack.getVolume(out float startVol);
        float curTime = 0;

        while (curTime < duration)
        {
            // If combat starts mid-fade, abort this coroutine immediately
            if (inCombat)
            {
                yield break;
            }

            curTime += Time.deltaTime;
            float t = curTime / duration;
            float smoothedT = Mathf.SmoothStep(0f, 1f, t);

            // Simple Lerp for the master volume fade
            float currentVol = Mathf.Lerp(startVol, targetVolume, smoothedT);
            curTrack.setVolume(currentVol);

            yield return null;
        }
        curTrack.setVolume(targetVolume);
    }
    #endregion

    #region Music Fades During Gameplay
    // this makes sure we can catch whenever the state is changing from combat to non combat and vice versa
    public void GameStateChanged()
    {
        GameStateManager.GameState gameState = GameStateManager.Instance.getGameState();

        RuntimeManager.StudioSystem.getParameterByName("Combat Track Volume", out float combatVol);

        // check game state to see if we are changing to peaceful or combat or apex combat
        if (gameState == GameStateManager.GameState.PEACEFUL)
        {
            // state changed and now its peaceful
            ToggleComabatVolume();
            curMusicState = MusicCycleState.Playing;
        }
        else if (gameState == GameStateManager.GameState.PURSUED)
        {
            ToggleComabatVolume();
            curMusicState = MusicCycleState.Combat;
        }
        else if (gameState == GameStateManager.GameState.PURSUED_BY_APEX)
        {
            if (curMusicState != MusicCycleState.ApexLurk)
            {
                curMusicState = MusicCycleState.ApexLurk;
                CrossFadeMusic("ApexLurk", 9f);
            }
        }
    }
    #endregion
}
