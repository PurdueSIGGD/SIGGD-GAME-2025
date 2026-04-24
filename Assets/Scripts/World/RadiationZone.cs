using UnityEngine;

public class RadiationZone : MonoBehaviour
{
    [SerializeField] int zoneLevel = 0; // 0 is least dangerous, 4 is most

    [HideInInspector]
    public RadioactiveVFXManager radioactiveVFXManager;

    private void Awake()
    {
        radioactiveVFXManager = gameObject.GetComponent<RadioactiveVFXManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRadiation playerRadiation = (PlayerRadiation) other.gameObject.GetComponent<PlayerRadiation>();
            playerRadiation.InRadiation = true;
            playerRadiation.RadiationZoneLevel = zoneLevel;


            playerRadiation.radiationZone = this;

            Debug.Log("player entered radiation zone");


            // If Radiation VFX container active and if the timer to deactivate the container is running, cancel
            if (radioactiveVFXManager.disableVFXCoroutine != null)
            {
                radioactiveVFXManager.CancelStop();
            } else
            {
                // Initialize VFX for the first time
                radioactiveVFXManager.Init();
            }


        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerRadiation>().InRadiation = false;
            Debug.Log("player left radiation zone");
        }
    }

    void OnDrawGizmos() // for editor visibility
    {
        Gizmos.color = Color.green;
        float radius = GetComponent<SphereCollider>().radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
