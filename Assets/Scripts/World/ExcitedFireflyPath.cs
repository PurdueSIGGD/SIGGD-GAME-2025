using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;

public class ExcitedFireflyPath : MonoBehaviour
{
    [SerializeField] GameObject goPath;
    [SerializeField] GameObject exitPath;

    private bool going = false;
    private bool exiting = false;
    private float timer = 0.0f;
    private Vector3 next;
    private Vector3 prev;
    private Vector3 start;
    private int index = 0;
    private List<Vector3> goPathVec = new List<Vector3>();
    private List<Vector3> exitPathVec = new List<Vector3>();

    private void Start()
    {
        start = transform.position;
        for(int i = 0; i < goPath.transform.childCount; i++)
        {
            goPathVec.Add(goPath.transform.GetChild(i).position);
        }
        for (int i = 0; i < exitPath.transform.childCount; i++)
        {
            exitPathVec.Add(exitPath.transform.GetChild(i).position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            going = true;
            exiting = false;
            index = 0;
            GetComponent<SphereCollider>().enabled = false;
            prev = start;
            next = goPathVec[index];
            Debug.Log("Excited Firefly Path Activated");
        }
    }

    private Vector3 LerpVector(Vector3 a, Vector3 b, float t)
    {
        return new Vector3(
            Mathf.Lerp(a.x, b.x, t),
            Mathf.Lerp(a.y, b.y, t),
            Mathf.Lerp(a.z, b.z, t)
        );
    }

    private void Update()
    {
        if (going)
        {
            timer += Time.deltaTime;
            transform.position = LerpVector(prev, next, timer/(16.0f/goPathVec.Count));
            if (timer >= 16f/(goPathVec.Count))
            {
                timer = 0.0f;
                if (index == goPath.transform.childCount - 1)
                {
                    going = false;
                    exiting = true;
                    index = 0;
                    prev = transform.position;
                    next = exitPathVec[index];
                    Debug.Log("Excited Firefly Exiting");
                }
                else
                {
                    index++;
                    prev = next;
                    next = goPathVec[index];
                }
            }
        }
        else if (exiting)
        {
            timer += Time.deltaTime;
            transform.position = LerpVector(prev, next, timer/16.0f);
            if (timer >= 16f)
            {
                timer = 0.0f;
                if (index == goPath.transform.childCount - 1)
                {
                    exiting = false;
                    index = 0;
                    prev = start;
                    GetComponent<SphereCollider>().enabled = true;
                    next = new Vector3(0, 0, 0);
                }
                else
                {
                    index++;
                    prev = next;
                    next = goPathVec[index];
                }
            }
        }
    }

}
