using FMOD;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;

public class HummingLightsSound : MonoBehaviour
{
    private static readonly string hummingLightSound = "HummingLights";
    EventInstance lightSound;
    IEnumerator Start()
    {
        while (!FMODEvents.Instance.Initialized)
        {
            yield return null;
        }
        var eventRef = FMODEvents.Instance.GetEventReferenceNoAsync(hummingLightSound);
        if (eventRef.IsNull)
        {
            UnityEngine.Debug.LogError("FMOD event is null: " + hummingLightSound);
            yield break;
        }
        lightSound = RuntimeManager.CreateInstance(eventRef);
        lightSound.set3DAttributes(
            RuntimeUtils.To3DAttributes(PlayerID.Instance.transform)
        );
        RuntimeManager.AttachInstanceToGameObject(
            lightSound,
            PlayerID.Instance.transform,
            PlayerID.Instance.GetComponent<Rigidbody>()
        );
        UnityEngine.Debug.Log("Playing light sound");
        lightSound.start();
    }

    private void Update()
    {
        
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
