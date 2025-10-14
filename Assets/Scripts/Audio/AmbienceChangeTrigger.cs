using UnityEngine;

public class AmbienceChangeTrigger : MonoBehaviour
{
    [Header("Parameter Change")]

    [SerializeField] private string parameterName;

    [SerializeField] private float parameterValue;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            AudioManager.Instance.SetAmbienceParameter(parameterName, parameterValue);
        }
    }
}
