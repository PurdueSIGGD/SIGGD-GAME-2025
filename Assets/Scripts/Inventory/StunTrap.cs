using UnityEngine;

public class StunTrap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody rb;
    [SerializeField] private float throwForce;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        PlayerID.Instance.stateMachine.IgnoreCollisionWithObject(this.gameObject, true);
        Throw();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Throw() {
        Debug.Log("Throw: " + PlayerID.Instance.cam.transform.forward + " " + PlayerID.Instance.rb.linearVelocity);
        rb.position = PlayerID.Instance.gameObject.transform.position;
        rb.linearVelocity = PlayerID.Instance.rb.linearVelocity;
        rb.AddForce(PlayerID.Instance.cam.transform.forward * throwForce, ForceMode.Impulse);
        
    }

    void OnCollisionEnter(Collision collision)
    {
        // handle collision with mobs
    }
}
