using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class HummingLightsSound : MonoBehaviour
{
    private static readonly string hummingLightSound = "HummingLights";
    EventInstance lightSound;
    void Start()
    {
        var eventRef = FMODEvents.Instance.GetEventReferenceNoAsync(hummingLightSound);

        lightSound = RuntimeManager.CreateInstance(eventRef);
        Debug.Log("Playing light sound");
        lightSound.start();
    }

    void OnDestroy()
    {
        if (lightSound.isValid())
        {
            lightSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            lightSound.release();
        }
    }
}
