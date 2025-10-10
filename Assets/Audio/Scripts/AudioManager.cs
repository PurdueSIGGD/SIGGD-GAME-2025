using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    private EventInstance ambienceEventInstance;

    private void Awake()
    {
        if (instance != null)
        {
            // this hopefully will never be seen
            UnityEngine.Debug.Log("more than one audio manager in the scene");
        }
        instance = this;
    }

    private void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambienceEventInstance = CreateEventInstance(ambienceEventReference);
        ambienceEventInstance.start();
    }

    public void SetAmbienceParameter(string parameterName, float parameterValue)
    {
        ambienceEventInstance.setParameterByName(parameterName, parameterValue);
    }

    private void Start()
    {
        InitializeAmbience(FMODEvents.instance.ambience);
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

    // NOTE: 3d attributes need to be set in order to play instances in 3d
    public ATTRIBUTES_3D configAttributes3D(Vector3 position, Vector3 velocity, Vector3 forward, Vector3 up)
    {
        VECTOR pos = new VECTOR { x = position.x, y = position.y, z = position.z };
        VECTOR vel = new VECTOR { x = velocity.x, y = velocity.y, z = velocity.z };
        VECTOR forw = new VECTOR { x = forward.x, y = forward.y, z = forward.z };
        VECTOR upAttr = new VECTOR { x = up.x, y = up.y, z = up.z };
        return new ATTRIBUTES_3D { position = pos, velocity = vel, forward = forw, up = upAttr };
    }
}