using UnityEngine;

public class ClearAmbientLight : MonoBehaviour
{
    [SerializeField] GameObject sun;
    [SerializeField] bool lights;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!lights)
            {
                RenderSettings.ambientLight = Color.black;
                RenderSettings.ambientIntensity = 0f;
                RenderSettings.reflectionIntensity = 0f;
                RenderSettings.fog = false;
                sun.SetActive(false);
                Debug.Log("turned off");
            }
            else
            {
                RenderSettings.ambientLight = Color.white;
                RenderSettings.ambientIntensity = 1f;
                RenderSettings.reflectionIntensity = 1f;
                RenderSettings.fog = true;
                sun.SetActive(true);
                Debug.Log("turned on");
            }
        }
    }
}
