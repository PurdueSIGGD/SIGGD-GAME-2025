using Sirenix.OdinInspector;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    [SerializeField]
    public GameObject VFXContainerPrefab;

    [SerializeField]
    public RenderTexture renderTexture;

    // Whether the particle is playing
    [ShowInInspector]
    private bool isActive = false;

    private VisualEffect particlesVFX;
    private GameObject instantiatedPrefab = null;

    public void Awake()
    {
        SetInactive();

    }

    public void Update()
    {
        if (!isActive && particlesVFX.HasAnySystemAwake())
        {
            SetInactive();
        } else if (isActive && !particlesVFX.HasAnySystemAwake())
        {
            SetActive();
        }
    }


    public void SetActive() {
        
        particlesVFX.Play();

    }

    public void SetInactive()
    {
        particlesVFX.Stop();
    }
}
