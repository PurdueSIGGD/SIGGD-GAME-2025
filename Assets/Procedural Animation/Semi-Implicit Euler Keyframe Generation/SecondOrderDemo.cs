using UnityEngine;

public class SecondOrderDemo : MonoBehaviour {
    [SerializeField] Transform target;
    [Range(0, 15)] public float frequency;
    [Range(0, 10)] public float damping;
    [Range(-5, 5)] public float response;
    SecondOrderDynamics positionFilter;
    SecondOrderDynamics rotationFilter;

    void Start() {
        positionFilter = new SecondOrderDynamics(frequency, damping, response, transform.position);
        rotationFilter = new SecondOrderDynamics(frequency, damping, response, transform.forward);
    }

    void Update() {
        positionFilter.ComputerKValues(frequency, damping, response);
        Vector3 yPos = positionFilter.Update(Time.deltaTime, target.position);
        transform.position = yPos;

        rotationFilter.ComputerKValues(frequency, damping, response);
        Vector3 yRot = rotationFilter.Update(Time.deltaTime, target.forward);
        transform.forward = yRot;
    }
}
