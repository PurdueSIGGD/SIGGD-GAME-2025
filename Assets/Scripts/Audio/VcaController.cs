using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class VcaController : MonoBehaviour
{
    private VCA Vca;
    public string 

    void Start()
    {
        Vca = RuntimeManager.GetVCA("vca:/tempMaster");
    }

    void Update()
    {
        
    }
}
