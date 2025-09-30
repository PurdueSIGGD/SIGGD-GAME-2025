using UnityEngine;

public class SecondOrderDemo : MonoBehaviour {
    [SerializeField] Transform target;
    [Range(0, 15)] public float frequency;
    [Range(0, 10)] public float damping;
    [Range(-5, 5)] public float response;
    SecondOrderDynamics filter;

    void Start() {
        filter = new SecondOrderDynamics(frequency, damping, response, transform.position);
    }

    void Update() {
        filter.ComputerKValues(frequency, damping, response);

        Vector3 y = filter.Update(Time.deltaTime, target.position);
        transform.position = y;
    }
}
