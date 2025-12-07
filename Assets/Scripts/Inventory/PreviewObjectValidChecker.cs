using System.Collections.Generic;
using UnityEngine;

public class PreviewObjectValidChecker : MonoBehaviour
{
    [SerializeField] private LayerMask invalidLayers;
    public bool IsValid { get; private set; } = true;
    // Keep track of colliding objects to handle multiple collisions
    [SerializeField] private HashSet<Collider> _collidingObjects = new HashSet<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object's layer is in the invalid layers
        if (((1 << other.gameObject.layer) & invalidLayers) != 0)
        {
            _collidingObjects.Add(other);
            IsValid = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the exited object's layer is in the invalid layers
        if (((1 << other.gameObject.layer) & invalidLayers) != 0)
        {
            _collidingObjects.Remove(other);
            IsValid = _collidingObjects.Count <= 0;
        }
    }
}
