using UnityEngine;

public class Lure : MonoBehaviour
{

    public float lifetime = 25f;
    public float radius = 30f;
    public float intensity = 6f;
    private void OnEnable()
    {
        LureManager.RegisterLure(this);
    }
    private void OnDisable()
    {
        LureManager.UnregisterLure(this);
    }
    void Start()
    {
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }
}
