using UnityEngine;

public class StunTrap : MonoBehaviour, Usable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody rb;
    [SerializeField] private float throwForce;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //rb.linearVelocity = PlayerID.Instance.gameObject.GetComponent<Rigidbody>().linearVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Throw() {
        Debug.Log("Throw: " + PlayerID.Instance.cam.transform.forward);
        Debug.Log(PlayerID.Instance.gameObject.transform.position + " " + rb.position);
        rb.position = PlayerID.Instance.gameObject.transform.position;
        rb.AddForce(PlayerID.Instance.cam.transform.forward * throwForce, ForceMode.Impulse);
        
    }

    public void Use() {
        Throw();
    }

    void OnCollisionEnter(Collision collision)
    {
        // handle collision with mobs
    }
}
