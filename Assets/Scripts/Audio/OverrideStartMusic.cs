using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class OverrideStartMusic : MonoBehaviour
{
    [SerializeField] EventReference music;
    private EventInstance musicInstance;

    void Start()
    {
        musicInstance = RuntimeManager.CreateInstance(music);
        musicInstance.start();
    }

    /// <summary>
    /// Stops and frees the title music. Safe to call more than once.
    /// </summary>
    public bool StopActiveMusic()
    {
        // checking for PLAYING missed the STARTING state, and a stopped-but-unreleased instance leaked
        if (!musicInstance.isValid())
        {
            return false;
        }

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicInstance = default;
        return true;
    }

    // An FMOD EventInstance is not tied to this GameObject, so without this it keeps playing after the
    // menu scene unloads by any path that doesn't call StopActiveMusic (new game, credits...)
    void OnDestroy()
    {
        StopActiveMusic();
    }
}