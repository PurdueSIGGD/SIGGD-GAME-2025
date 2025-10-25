using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private Light flashlight;

    void Start()
    {
        flashlight = GetComponent<Light>();        

        if (flashlight == null)
        {
            Debug.LogWarning("Light component is not attached. Attach a Light component manually.");
            return;
        }

        flashlight.enabled = false;
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (flashlight != null)
            {
                flashlight.enabled = !flashlight.enabled;

                // play audio effect code here for turning flashlight on/off
            }
        }
    }
}
