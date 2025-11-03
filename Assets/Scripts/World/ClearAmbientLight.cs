using UnityEngine;

public class ClearAmbientLight : MonoBehaviour
{
    [SerializeField] GameObject sun;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientIntensity = 0f;
            sun.SetActive(false);
            Debug.Log("turned off");
        }
    }
}
