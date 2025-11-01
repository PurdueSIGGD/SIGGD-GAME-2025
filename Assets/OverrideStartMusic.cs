using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class OverrideStartMusic : MonoBehaviour
{
    [SerializeField] EventReference music;

    void Start()
    {
        RuntimeManager.PlayOneShot(music);
    }
}
