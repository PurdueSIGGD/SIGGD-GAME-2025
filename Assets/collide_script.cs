using UnityEngine;

public class collide_script : MonoBehaviour
{
    public look_at_player lookScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            lookScript.setActive(true);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            lookScript.setActive(false);
        }
    }
}
