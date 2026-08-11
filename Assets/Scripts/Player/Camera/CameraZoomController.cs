using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    public static CameraZoomController Instance { get; private set; }

    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float zoomSpeed = 60f; // FOV units per second

    private Camera playerCamera;
    private float targetFOV;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerCamera = GetComponent<Camera>();
        targetFOV = defaultFOV;
        playerCamera.fieldOfView = defaultFOV;
    }

    private void Update()
    {
        playerCamera.fieldOfView = Mathf.MoveTowards(
            playerCamera.fieldOfView,
            targetFOV,
            zoomSpeed * Time.deltaTime
        );
    }

    public void SetFOV(float newFOV)
    {
        targetFOV = newFOV;
    }

    public void ResetFOV()
    {
        targetFOV = defaultFOV;
    }

    public bool IsZoomed()
    {
        return targetFOV != defaultFOV;
    }
}