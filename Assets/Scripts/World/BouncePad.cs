using System.Collections;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 10f;
    [SerializeField] private float cooldownTime = 1f;
    [SerializeField] private bool destroysSelfOnBounce = false;
    private bool cooldown;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRigidbody != null && cooldown == false)
            {
                cooldown = true; 
                // Apply an upward force to the player
                playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0f, playerRigidbody.linearVelocity.z); // Reset vertical velocity
                playerRigidbody.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

                if (destroysSelfOnBounce)
                {
                    Destroy(gameObject);
                }
                else
                {
                    // Start cooldown coroutine
                    StartCoroutine(CooldownWait());
                }

            }
            
        }
    }

    IEnumerator CooldownWait()
    {
        yield return new WaitForSeconds(cooldownTime);
        cooldown = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

   
}
