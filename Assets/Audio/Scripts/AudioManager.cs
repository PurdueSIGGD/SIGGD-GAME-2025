using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            // this hopefully will never be seen
            Debug.Log("more than one audio manager in the scene");
        }
        instance = this;
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
}
