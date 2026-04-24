using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UI;

public class ControllerVca : MonoBehaviour
{
    private VCA VCA;
    public string vcaName;

    private Slider slider;

    private float GetVolume()
    {
        VCA.getVolume(out float volume);
        return volume;
    }

    void Start()
    {
        VCA = RuntimeManager.GetVCA("vca:/" + vcaName);
        slider = GetComponent<Slider>(); // gets the slider that this script is attached to6s
        slider.value = GetVolume();
    }

    // This just sets the slider to the new VCA volume, if it was changed by something that wasn't the slider.
    public void Restart()
    {
        slider.value = GetVolume();
    }

    public void changeVolume(float volume)
    {
        VCA.setVolume(volume);
    }
}
