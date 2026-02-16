using UnityEngine;
using System;

public class LookFollow : MonoBehaviour
{

    public Func<Vector3> player;
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);

    void LateUpdate()
    {
        if (player == null) return;
        transform.position = player() + offset;
    }
}
