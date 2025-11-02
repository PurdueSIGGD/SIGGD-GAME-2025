using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class OverrideStartMusic : MonoBehaviour
{
    [SerializeField] EventReference music;
    private EventInstance musicInstance;

    void Start()
    {
        //RuntimeManager.PlayOneShot(music);
        musicInstance = RuntimeManager.CreateInstance(music);
        musicInstance.start();
    }

    public bool StopActiveMusic()
    {
        PLAYBACK_STATE state;
        musicInstance.getPlaybackState(out state);
        if (state == PLAYBACK_STATE.PLAYING)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
            return true;
        }
        return false;
    }
}
