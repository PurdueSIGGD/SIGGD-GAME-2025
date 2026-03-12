using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class ThrowItem : MonoBehaviour
{  
    public static ThrowItem Instance { get; private set; }
    private Camera playerCam;
    private void Awake()
    {
        // singleton stuff
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }
    void Start()
    {
        playerCam = PlayerID.Instance.cam.GetComponentInChildren<Camera>();
    }
    public void Throw(GameObject projectile, float throwForce)
    {
        // creates the projectile and applies force in the direction of the camera
        GameObject createdProj = Instantiate(projectile, playerCam.transform.position + playerCam.transform.forward, transform.rotation);
        createdProj.GetComponent<Rigidbody>().AddForce(playerCam.transform.forward * throwForce, ForceMode.VelocityChange);
    }
    public void ThrowBait(GameObject projectile, float throwForce, float radius, float duration)
    {
        // creates the projectile and applies force in the direction of the camera
        GameObject createdProj = Instantiate(projectile, playerCam.transform.position + playerCam.transform.forward, transform.rotation);
        createdProj.GetComponent<Rigidbody>().AddForce(playerCam.transform.forward * throwForce, ForceMode.VelocityChange);
    }
}
