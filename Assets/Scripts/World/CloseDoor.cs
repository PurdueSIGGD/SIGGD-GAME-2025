using UnityEngine;

public class CloseDoor : MonoBehaviour
{
    [SerializeField] private GameObject blocker;
    [SerializeField] private GameObject otherSide;
    [SerializeField] private GameObject homeBase;
    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.CompareTag("Player")) {
        if (blocker != null)
            blocker.SetActive(true);
        if (otherSide != null)
            otherSide.SetActive(true);
        if (homeBase != null)
            homeBase.SetActive(true);
        //}
    }
}
