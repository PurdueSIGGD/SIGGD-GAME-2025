using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class RadioactiveVFXManager : MonoBehaviour
{
    // How many seconds to wait after Player leaves radiation zone to deactivate the container
    private const int DISACTIVATE_WAIT_TIME = 5; 

    [SerializeField]
    private GameObject container; // container holds VFX, camera, and canvas
    [SerializeField]
    private Vector3 spawnLocation; // where container will spawn
    [SerializeField]
    private Image renderImage; // Image with render texture inside canvas in container
    [SerializeField]
    private GameObject VFXGameObject; // Game object with VFX component

    public Coroutine disableVFXCoroutine = null;

    private VisualEffect particlesVFX; // Particles VFX in container

    private void Start()
    {
        particlesVFX = VFXGameObject.GetComponent<VisualEffect>();
        container.transform.position = spawnLocation;
        container.SetActive(false);
        particlesVFX.Stop();
    }

    /// <summary>
    /// Create the container prefab
    /// </summary>
    public void Init()
    {
        if (IsRunning()) return;
        container.SetActive(true);
        particlesVFX.Play();
        Debug.Log("Activated VFX container");
    }

    /// <summary>
    /// Update the opacity of the render texture canvas based on percent
    /// </summary>
    /// <param name="percentage"></param>
    public void UpdateOpacity(float percentage)
    {
        if (IsRunning()) {
            Color c = renderImage.color;
            c.a = percentage;
            renderImage.color = c;
        }
    }

    // Disable container after radiation is 0 for some time
    private IEnumerator DisableVFX()
    {
        yield return new WaitForSeconds(DISACTIVATE_WAIT_TIME);
        particlesVFX.Stop();
        container.SetActive(false);
        disableVFXCoroutine = null;
        Debug.Log("disactivated container for VFX");
    }

    public void StopAfterDelay()
    {
        if (disableVFXCoroutine == null)
        {
            disableVFXCoroutine = StartCoroutine(DisableVFX());
        }
    }

    public void CancelStop()
    {
        if (disableVFXCoroutine != null)
        {
            StopCoroutine(disableVFXCoroutine);
            disableVFXCoroutine = null;
        }
    }

    public bool IsRunning()
    {
        return container.activeSelf;
    }
}
