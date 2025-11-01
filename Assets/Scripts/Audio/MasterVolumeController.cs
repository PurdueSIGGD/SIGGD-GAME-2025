using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class VcaController : MonoBehaviour
{
    public string VcaName; // so that this script can be put on every slider

    private VCA VCA;

    void Start()
    {
        VCA = RuntimeManager.GetVCA("vca:/" + VcaName);
    }

    void Update()
    {
        
    }
}
