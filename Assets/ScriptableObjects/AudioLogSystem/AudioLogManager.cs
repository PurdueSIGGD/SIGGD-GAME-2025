using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;
using FMOD;

public class AudioLogManager : Singleton<AudioLogManager>
{
    [SerializeField] AudioLogObject[] logs;
    [SerializeField] TextMeshProUGUI subtitles;
    private Coroutine lastStarted = null;

    public static AudioLogManager Instance { get; set; }

    private EventInstance logSoundEvent;

    [HideInInspector] public GameObject curPlayer; // to store player object after playAudioLog stops running
    private Rigidbody playerRb;

    [SerializeField] private bool isPlaying = false;

    private Dictionary<string, AudioLogObject> audioNameToLogs = new();
    [HideInInspector] public List<string> names = new List<string>();


    void Awake()
    {
        if (Instance != null)
        {
            UnityEngine.Debug.Log("Error, too many AudioLogManagers in scene");
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var log in logs)
        {
            audioNameToLogs[log.audioName] = log;
            names.Add(log.audioName);
        }
    }

    private void Update()
    {
        if(isPlaying && curPlayer != null)
        {
            ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(playerRb.position, playerRb.linearVelocity, playerRb.transform.forward, playerRb.transform.up);
            logSoundEvent.set3DAttributes(attr);
        }
    }

    private IEnumerator StartSubtitles(AudioLogObject curAudio)
    {
        subtitles.enabled = true;

        foreach (var line in curAudio.subtitles)
        {
            subtitles.text = line.line;
            yield return new WaitForSeconds(line.seconds);
        }
        subtitles.enabled = false;
        lastStarted = null;
        isPlaying = false;
        curPlayer = null;

        logSoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        logSoundEvent.release();
    }

    public void PlayAudioLog(string audioName) // using a full game object because we need access to the rigidbody on the player
    {
        GameObject player = PlayerID.Instance.gameObject;
        // the most recently called audio log will take priority over the ones called before it 
        if (lastStarted != null)
        {
            UnityEngine.Debug.Log("audio log already started stopping old one before playing new one");
            StopCoroutine(lastStarted);
            StopCurrentAudio();
        }

        if (audioNameToLogs.TryGetValue(audioName, out var foundAudio) && !isPlaying)
        {
            UnityEngine.Debug.Log("Playing audio log: " + audioName);

            curPlayer = player;
            isPlaying = true;
            playerRb = curPlayer.GetComponent<Rigidbody>();

            // now that isPlaying is true and logSoundEvent exists the 3d attributes will be getting updated and we can start the event
            AudioManager.Instance.PlayOneShot(audioName);

            lastStarted = StartCoroutine(StartSubtitles(foundAudio));

            //StartCoroutine(endAudioWhenDone());
        }
        else
        {
            UnityEngine.Debug.Log("Audio name not in dictionarty: " + audioName);
        }
    }

    public void PlayAudioLog (string audioName, GameObject player) // using a full game object because we need access to the rigidbody on the player
    {
        // the most recently called audio log will take priority over the ones called before it 
        if (lastStarted != null)
        {
            StopCoroutine(lastStarted);
            StopCurrentAudio();
        }

        if (audioNameToLogs.TryGetValue(audioName, out var foundAudio) && !isPlaying)
        {
            curPlayer = player;
            isPlaying = true;
            playerRb = curPlayer.GetComponent<Rigidbody>();

            // now that isPlaying is true and logSoundEvent exists the 3d attributes will be getting updated and we can start the event
            AudioManager.Instance.PlayOneShot(audioName, player.transform.position);

            lastStarted = StartCoroutine(StartSubtitles(foundAudio));

            //StartCoroutine(endAudioWhenDone());
        }
        else
        {
            UnityEngine.Debug.Log("Audio name not in dictionarty: " + audioName);
        }
    }

    //// this will end the audio naturally once the clip is done playing if its not interrupted
    //private IEnumerator endAudioWhenDone()
    //{
    //    PLAYBACK_STATE state;

    //    // for out current testing this wont work because footsteps doesnt end it just loops but this should work for more real events that we'll implement
    //    logSoundEvent.getPlaybackState(out state);
    //    while (state != PLAYBACK_STATE.STOPPED)
    //    {
    //        yield return null;
    //    }

    //    isPlaying = false;
    //    curPlayer = null;
    //    logSoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    //    logSoundEvent.release();
    //}

    // this can be used for interrupt
    public void StopCurrentAudio()
    {
        // itll break if we try to stop stuff while nothing is playing
        if (!isPlaying)
        {
            return;
        }

        // run all the normal stop stuff including stopping audio
        logSoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        logSoundEvent.release(); // stops the now unused event from floating around not doing anything
        isPlaying = false;
        curPlayer = null;

        subtitles.enabled = false;
        lastStarted = null;
    }
}