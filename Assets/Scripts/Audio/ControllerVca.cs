using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UI;

public class ControllerVca : MonoBehaviour
{
    private VCA VCA;
    public string vcaName;

    private Slider slider;

    void Start()
    {
        VCA = RuntimeManager.GetVCA("vca:/" + vcaName);
        slider = GetComponent<Slider>(); // gets the slider that this script is attached to6s
    }

    public void changeVolume(float volume)
    {
        VCA.setVolume(volume);
    }
}
