using System;
using UnityEngine;

/**
 * Attach to a GameObject to listen for trigger events and invoke corresponding actions.
 */
public class TriggerListener : MonoBehaviour
{
    public Action<Collider> onTriggerEnter = delegate { };
    public Action<Collider> onTriggerStay = delegate { };
    public Action<Collider> onTriggerExit = delegate { };
    
    public void OnTriggerEnter(Collider other)
    {
        onTriggerEnter?.Invoke(other);
    }
    
    public void OnTriggerStay(Collider other)
    {
        onTriggerStay?.Invoke(other);
    }

    public void OnTriggerExit(Collider other)
    {
        onTriggerExit?.Invoke(other);
    }
}