using NUnit.Framework;
using UnityEngine;
using UnityEngine.VFX;

public class RadioactiveVFXManager : MonoBehaviour
{

    [SerializeField]
    private GameObject container; // container holds VFX, camera, and canvas
    [SerializeField]
    private Vector3 spawnLocation; // where container will spawn
    [SerializeField]
    private RenderTexture renderTexture; // render texture in container
    [SerializeField]
    private GameObject VFXGameObject; // Game object with VFX component


    private VisualEffect particlesVFX; // Particles VFX in container

    private void Start()
    {
        particlesVFX = VFXGameObject.GetComponent<VisualEffect>();
        container.transform.position = spawnLocation;
        Stop();
    }

    /// <summary>
    /// Create the container prefab
    /// </summary>
    public void Init()
    {  
        container.SetActive(true);
        particlesVFX.Play();
    }

    /// <summary>
    /// Update the opacity of the render texture based on percent
    /// </summary>
    /// <param name="percentage"></param>
    public void UpdateOpacity(float percentage)
    {
        Debug.Log("update opacity todo " + percentage);
    }

    // Remove container
    public void Stop()
    {
        particlesVFX.Stop();
        container.SetActive(false);
    }
}
