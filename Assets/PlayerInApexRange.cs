using UnityEngine;

public class PlayerInApexRange : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("the trigger is crossed");
        if (collider.CompareTag("Apex"))
        {
            Debug.Log("hit apex hitbox trigger apex lurk music");
        }
    }
}
