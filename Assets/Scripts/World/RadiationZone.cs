using UnityEngine;

public class RadiationZone : MonoBehaviour
{
    [SerializeField] int zoneLevel = 0; // 0 is least dangerous, 4 is most

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("entered radiation zone");
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerRadiation>().InRadiation = true;
            other.gameObject.GetComponent<PlayerRadiation>().RadiationZone = zoneLevel;
            Debug.Log("player entered radiation zone");
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
