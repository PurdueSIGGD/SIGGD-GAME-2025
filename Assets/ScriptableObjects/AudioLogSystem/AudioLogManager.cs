using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;
using FMOD;

public class AudioLogManager : MonoBehaviour
{
    [SerializeField] AudioLogObject[] logs;
    [SerializeField] TextMeshProUGUI subtitles;
    private Coroutine lastStarted = null;

    public static AudioLogManager Instance { get; set; }

    private EventInstance logSoundEvent;

    public GameObject curPlayer; // to store player object after playAudioLog stops running
    private Rigidbody playerRb;

    [SerializeField] private bool isPlaying = false;

    private Dictionary<string, AudioLogObject> audioNameToLogs = new();
    public List<string> names = new List<string>();


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

    private IEnumerator startSubtitles(AudioLogObject curAudio)
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
    public void playAudioLog (string audioName, GameObject player) // using a full game object because we need access to the rigidbody on the player
    {
        if (lastStarted != null)
        {
            StopCoroutine(lastStarted);
            StopCurrentAudio();
        }

        if (audioNameToLogs.TryGetValue(audioName, out var foundAudio) && !isPlaying)
        {
            logSoundEvent = FMODEvents.instance.getEventInstanceNOASYNC(audioName); // audio shouldnt need to be delayed since its not being called in the start
            curPlayer = player;
            isPlaying = true;
            playerRb = curPlayer.GetComponent<Rigidbody>();

            // now that isPlaying is true nad logSoundEvent exists the 3d attributes will be getting updated and we can start the event
            logSoundEvent.start();

            lastStarted = StartCoroutine(startSubtitles(foundAudio));

            //StartCoroutine(endAudioWhenDone());
        }
        else
        {
            UnityEngine.Debug.Log("Audio name not in dictionarty: " + audioName);
        }
    }

    // this will end the audio naturally once the clip is done playing if its not interrupted
    private IEnumerator endAudioWhenDone()
    {
        PLAYBACK_STATE state;

        // for out current testing this wont work because footsteps doesnt end it just loops but this should work for more real events that we'll implement
        logSoundEvent.getPlaybackState(out state);
        while (state != PLAYBACK_STATE.STOPPED)
        {
            yield return null;
        }

        isPlaying = false;
        curPlayer = null;
        logSoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        logSoundEvent.release();
    }

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