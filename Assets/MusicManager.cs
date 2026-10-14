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
    private string currentTrackKey = "";

    [Header("This is the music you want playing when the game starts not when this scene is loaded")]
    [SerializeField] private string initMusicKey;

    [ShowInInspector, ReadOnly] public Dictionary<string, EventInstance> musicEventInstances = new();
    
    private Coroutine crossFadeRoutine;
    private bool keepTrackThroughNextSceneLoad;
    private bool skipSceneMusicOnce;
    private Coroutine activeFadeRoutine;
    private Coroutine pauseMusicRoutine;
    private Coroutine playMusicRoutine;

    public enum MusicCycleState { Playing, FadingOut, FadingIn, NotPlaying, Combat, Stopped, ApexLurk}

    public MusicCycleState curMusicState = MusicCycleState.NotPlaying;

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
            _instance.UpdateManagerParam(initLevelMusic, initMusicKey);
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
        if (keepTrackThroughNextSceneLoad)
        {
            // the track was asked to carry over into this scene, so it also replaces the scene's own music
            keepTrackThroughNextSceneLoad = false;
            skipSceneMusicOnce = true;
            return;
        }
        skipSceneMusicOnce = false;

        // a crossfade from the old scene would keep pushing volume on a track we are about to stop
        if (crossFadeRoutine != null)
        {
            StopCoroutine(crossFadeRoutine);
            crossFadeRoutine = null;
        }

        // Stop EVERY music track, not just curTrack. A track that stopped being curTrack without being
        // stopped (replaced by SetCurTrack, the init music, a cassette...) would otherwise keep playing
        // forever and stack with the next scene's music.
        StopAllMusic();
        curMusicState = MusicCycleState.Stopped;
    }

    /// <summary>
    /// Stops every cached music instance. Instances stay in the dictionary so they can be started again.
    /// </summary>
    private void StopAllMusic()
    {
        foreach (var instance in musicEventInstances.Values)
        {
            if (instance.isValid())
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }

    /// <summary>
    /// Makes the given instance the current track, stopping the previous one if it is a different track.
    /// Every place that replaces curTrack outside of a crossfade should go through this, otherwise the
    /// old track is orphaned: still playing, but no longer reachable through curTrack to stop it.
    /// </summary>
    private void SwitchCurTrack(EventInstance next)
    {
        if (curTrack.isValid() && curTrack.handle != next.handle)
        {
            curTrack.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        curTrack = next;
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
    public void UpdateManagerParam(bool initLevelMusic, string initMusicKey)
    {
        // take the new scene's key too, otherwise the init music would replay the key from whichever
        // scene the game was first launched in (e.g. main menu music starting inside the game)
        this.initMusicKey = initMusicKey;

        if (this.initLevelMusic != initLevelMusic)
        {
            this.initLevelMusic = initLevelMusic;
            InitMusicOnStart();
        }
    }

    /// <summary>
    /// True while one track is fading into another. A new crossfade asked for during this time is ignored.
    /// </summary>
    public bool IsCrossFading => crossFadeRoutine != null;

    /// <summary>
    /// Lets the current track keep playing through the next scene load instead of being stopped, and
    /// stops that scene from starting its own music over it. Applies to one scene load only.
    /// </summary>
    public void KeepTrackThroughNextSceneLoad()
    {
        keepTrackThroughNextSceneLoad = true;
    }

    /// <summary>
    /// Asked by the scene's music link before it starts the scene music. Returns true once after a track
    /// was carried over into the scene, meaning the scene music should not start.
    /// </summary>
    public bool ConsumeSkipSceneMusic()
    {
        bool skip = skipSceneMusicOnce;
        skipSceneMusicOnce = false;
        return skip;
    }

    /// <summary>
    /// Overrides curTrack with the key you passed in
    /// </summary>
    public void SetCurTrack(string key)
    {
        Debug.Log("Cur Track is " + key);
        SwitchCurTrack(InitalizeMusicNotStart(key));
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
        if (toKey.ToLower() == currentTrackKey)
        {
            return;
        }

        Debug.Log("crossfading music");
        if (crossFadeRoutine != null)
        {
            //StopCoroutine(crossFadeRoutine);
            return;
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
    
    /// <summary>
    /// 
    /// </summary>
    public IEnumerator playCassette(string cassetteKey)
    {
        Debug.Log("in cassette");
        cassetteKey = cassetteKey.ToLower();

        EventInstance cassetteEvent = InitalizeMusicNotStart(cassetteKey);
        cassetteEvent.getDescription(out EventDescription cassetteDescription);
        cassetteDescription.getLength(out int length);

        float lengthSec = length / 1000;

        curTrack.setPaused(true);

        cassetteEvent.start();

        yield return new WaitForSeconds(lengthSec);

        cassetteEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        curTrack.setPaused(false);
    }

    #endregion

    #region Private Methods
    private void InitMusicOnStart()
    {
        StartCoroutine(InitMusicOnStartCoroutine());
    }

    private IEnumerator PlayMusicOneShotCoroutine(string name, Vector3 pos = default)
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
        Debug.Log("IN CROSSFADE");
        toKey = toKey.ToLower();

        EventInstance to = InitalizeMusicNotStart(toKey);

        // we are always crossfading from the current track to something
        EventInstance from = curTrack;

        curTrack = to;

        float curTime = 0f;

        to.setVolume(0f);

        // if to isnt already playing play it
        to.getPlaybackState(out PLAYBACK_STATE state);
        if (state != PLAYBACK_STATE.PLAYING)
        {
            to.start();
        }

        while (curTime < duration)
        {
            Debug.Log("FADING");
            curTime += Time.deltaTime; // because its framebased it could cause issues but that fine for now
            float t = Mathf.Clamp01(curTime / duration);

            float smoothedT = Mathf.SmoothStep(0f, 1f, t);

            // 3. Equal Power Crossfade Math
            // Fade In: sin(t * pi/2)
            // Fade Out: cos(t * pi/2)
            float fadeInVol = Mathf.Sin(smoothedT * Mathf.PI * 0.5f);
            float fadeOutVol = Mathf.Cos(smoothedT * Mathf.PI * 0.5f);

            to.setVolume(fadeInVol);

            if (from.isValid())
            {
                from.setVolume(fadeOutVol);
            }

            yield return null; // wait for a frame in between loop runs
        }

        to.setVolume(1f);

        if (from.isValid())
        {
            from.setVolume(0f);
            from.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        Debug.Log("setting the routine to null");
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
            if (!string.IsNullOrEmpty(initMusicKey) && musicEventInstances.TryGetValue(initMusicKey.ToLower(), out var eventInstance))
            {
                SwitchCurTrack(eventInstance);

                // the scene's MusicSceneLink may have already started this same track; don't restart it
                curTrack.getPlaybackState(out PLAYBACK_STATE state);
                if (state != PLAYBACK_STATE.PLAYING && state != PLAYBACK_STATE.STARTING)
                {
                    curTrack.start();
                }
                curTrack.setPaused(false);
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

    // this makes sure we can catch whenever the state is changing from combat to non combat and vice versa
    public void GameStateChanged()
    {
        Debug.Log("game state changed");
        GameStateManager.GameState gameState = GameStateManager.Instance.getGameState();

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
            // play apex fight music?
        }
    }
}
