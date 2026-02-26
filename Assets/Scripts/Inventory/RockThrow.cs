using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class RockThrow : MonoBehaviour
{  
    public static RockThrow Instance { get; private set; }
    public float defaultThrowForce;
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
    public void ThrowRock(GameObject projectile)
    {
        Debug.Log("The script is working :)");
        GameObject createdProj = Instantiate(projectile, playerCam.transform.position + playerCam.transform.forward, transform.rotation);
        createdProj.GetComponent<Rigidbody>().AddForce(playerCam.transform.forward * defaultThrowForce, ForceMode.VelocityChange);
    }
}
