using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using FMODUnity;
using FMOD.Studio;
using FMOD;
using Debug = UnityEngine.Debug;

public class AudioLogManager : MonoBehaviour
{
    public List<AudioLogObject> logs = new();
    [SerializeField] TextMeshProUGUI subtitles;
    private Coroutine lastStarted = null;

    [SerializeField] private Animator anim;
    public static AudioLogManager Instance { get; set; }

    private EventInstance logSoundEvent;

    [HideInInspector] public GameObject curPlayer; // to store player object after playAudioLog stops running
    private Rigidbody playerRb;

    [SerializeField] private bool isPlaying = false;

    private Dictionary<string, AudioLogObject> audioNameToLogs = new();
    [HideInInspector] public List<string> names = new List<string>();

    //private float total_time = 0.0f; // to un cumulative the time stamps as we play them
    void Awake()
    {
        if (Instance != null)
        {
            UnityEngine.Debug.LogWarning("Error, too many AudioLogManagers in scene");
            return;
        }
        Instance = this;
    }

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
        if (isPlaying)
        {
            StopCurrentAudio();
        }
    }

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
        if (isPlaying && curPlayer != null)
        {
            ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(playerRb.position, playerRb.linearVelocity, playerRb.transform.forward, playerRb.transform.up);
            logSoundEvent.set3DAttributes(attr);
        }
    }

    private IEnumerator StartSubtitles(AudioLogObject curAudio)
    {
        logSoundEvent.start();

        subtitles.enabled = true;
        bool prevWasFromRadio = false;
        bool prevWasIntoRadio = false;

        foreach (var line in curAudio.subtitles)
        {
            // plays the activate noise for the radio based on if the prev is different from the cur and if its to or from
            if (line.isFromRadio && !prevWasFromRadio)
            {
                logSoundEvent.setPaused(true);
                AudioManager.Instance.PlayOneShot("radionoiselong", curPlayer.transform.position);
                yield return new WaitForSeconds(2.0f);
                logSoundEvent.setPaused(false);
            }
            else if (line.isIntoRadio && !prevWasIntoRadio)
            {
                logSoundEvent.setPaused(true);
                AudioManager.Instance.PlayOneShot("radionoise", curPlayer.transform.position);
                yield return new WaitForSeconds(0.6f);
                logSoundEvent.setPaused(false);
            }

            // put away radio and play deactivate noise logic
            if ((prevWasFromRadio || prevWasIntoRadio) && (!line.isFromRadio && !line.isIntoRadio))
            {
                logSoundEvent.setPaused(true);
                AudioManager.Instance.PlayOneShot("radiodeactivatenoise", curPlayer.transform.position);
                yield return new WaitForSeconds(0.75f);
                logSoundEvent.setPaused(false);
            }

            anim.SetBool("LeftVisible", line.isFromRadio || line.isIntoRadio);
            if (line.isFromRadio || line.isIntoRadio)
            {
                anim.Play("PlayerHand_Left_Idle");
            }

            RuntimeManager.StudioSystem.setParameterByName("RadioVoice", line.isFromRadio ? 1 : 0);

            subtitles.text = line.line;

            prevWasFromRadio = line.isFromRadio;
            prevWasIntoRadio = line.isIntoRadio;

            yield return new WaitForSeconds(line.seconds);
        }

        if (prevWasIntoRadio || prevWasFromRadio)
        {
            AudioManager.Instance.PlayOneShot("radiodeactivatenoise", curPlayer.transform.position);
            yield return new WaitForSeconds(0.75f);
        }

        StopCurrentAudio();
    }

    public void PlayAudioLog(string audioName, GameObject player, bool subtitles) // using a full game object because we need access to the rigidbody on the player
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

            audioName = audioName.ToLower();

            // get the sound event from our dictionary and store it
            if (FMODEvents.Instance.soundEvents.TryGetValue(audioName, out EventReference eventRef))
            {
                logSoundEvent = RuntimeManager.CreateInstance(eventRef);
            }

            lastStarted = StartCoroutine(StartSubtitles(foundAudio));
        }
        else
        {
            Debug.LogWarning("Audio name not in dictionarty: " + audioName);
        }
    }

    // this can be used for interruptting the current voice line (monster attack, etc.)
    public void StopCurrentAudio()
    {
        // itll break if we try to stop stuff while nothing is playing
        if (!isPlaying)
        {
            return;
        }

        StopCoroutine(lastStarted);

        RuntimeManager.StudioSystem.setParameterByName("RadioVoice", 0);

        // run all the normal stop stuff including stopping audio
        anim.SetBool("LeftVisible", false);
        logSoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        logSoundEvent.release(); // stops the now unused event from floating around not doing anything
        isPlaying = false;
        curPlayer = null;

        subtitles.enabled = false;
        lastStarted = null;
    }
}