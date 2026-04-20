using UnityEngine;

public class BaitTrigger : MonoBehaviour
{
    public ActivateBait bait;

    private void OnTriggerEnter(Collider other)
    {
        bait.OnBaitTriggerEnter(other);
    }
}