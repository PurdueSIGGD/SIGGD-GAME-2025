using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public static FlashlightController Instance { get; private set; }
    public GameObject flashlight; // Assign this in the Unity Inspector

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        
    }

    public void ToggleFlashlight()
    {
        // Implement flashlight toggle logic here
        Debug.Log("Flashlight toggled");
        if (flashlight != null)
        {
            flashlight.SetActive(!flashlight.activeSelf);
        }
        else
        {
            Debug.LogWarning("Flashlight GameObject is not assigned in the Inspector.");
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
