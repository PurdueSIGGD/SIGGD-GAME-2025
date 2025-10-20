using UnityEngine;

public class LegTestScript : MonoBehaviour {
    [SerializeField] Transform origin;
    [SerializeField] Transform target;
    [SerializeField] float stepHeight = 1f;
    [SerializeField, Range(0, 1)] float t;


    void Update() {
        float height = Mathf.Sin(Mathf.PI * t) * stepHeight;

        transform.position = Vector3.Lerp(origin.position, target.position, t) + Vector3.up * height;
    }
}
