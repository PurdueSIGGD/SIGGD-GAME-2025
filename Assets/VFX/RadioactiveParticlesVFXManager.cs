using UnityEngine;
using UnityEngine.VFX;

public class RadioactiveParticlesVFXManager : MonoBehaviour
{

    VisualEffect particlesVFX = null;

    private void Awake()
    {
        particlesVFX = GetComponent<VisualEffect>();
    }
    void OnPlay()
    {
        particlesVFX.Reinit();
        particlesVFX.Play();
    }

    void OnStop()
    {
        particlesVFX?.Stop();
    }

    
}
