using UnityEngine;
using System;

public class LookFollow : MonoBehaviour
{

    public GameObject player = null;

    private void Start()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 100);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                player = collider.gameObject;
            }
        }
    }
    void LateUpdate()
    {
        if (player == null) return;
        transform.position = player.transform.position;
    }
}
