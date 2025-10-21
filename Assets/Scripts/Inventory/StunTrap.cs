using UnityEngine;

public class StunTrap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody rb;
    [SerializeField] private float throwForce;
    [SerializeField] GameObject camera;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Throw();
        }
    }

    void Throw() {
        Debug.Log("Throw: " + PlayerID.Instance.cam.transform.forward);
        rb.position = PlayerID.Instance.gameObject.transform.position;
        rb.AddForce(PlayerID.Instance.cam.transform.forward * throwForce, ForceMode.Impulse);
        
    }

    void OnCollisionEnter(Collision collision)
    {
        // handle collision with mobs
    }
}
