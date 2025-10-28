using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;

public class AudioLogManager : MonoBehaviour
{
    [SerializeField] AudioLogObject[] logs;
    [SerializeField] TextMeshProUGUI subtitles;
    private Coroutine lastStarted = null;

    public static AudioLogManager Instance { get; set; }

    private Dictionary<string, AudioLogObject> audioNameToLogs = new();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Error, too many AudioLogManagers in scene");
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
    }
    public void playOneShot (string audioName, Transform location)
    {
        if (lastStarted != null)
        {
            StopCoroutine(lastStarted);
        } 
        FMODEvents.instance.playOneShot(audioName, location.position);
        if (audioNameToLogs.TryGetValue(audioName, out var foundAudio))
        {
            lastStarted = StartCoroutine(startSubtitles(foundAudio));
        }
        else
        {
            Debug.Log("Audio name not in dictionarty: " + audioName);
        }
    }
}
