using UnityEngine;

public class ChangeFootstepSound : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerID.Instance.gameObject) {
            PlayerID.Instance.playerMovement.SwitchFootstepSound();
        }
    }
}
