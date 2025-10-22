using SIGGD.Goap.Behaviours;
using SIGGD.Goap.Capabilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Smell : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float smellRadius;
    public LayerMask targetMask;
    public LayerMask smellReductionMask;
    public float targetSmellIntensity;
    public GameObject targetRef;
    private HungerBehaviour HungerBehaviour;


    void Awake()
    {
        HungerBehaviour = GetComponent<HungerBehaviour>();
    }
    void Start()
    {
        StartCoroutine(SmellRoutine());
    }

    // Update is called once per frame
    void Update()
    {

    }
    private IEnumerator SmellRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);

        while (true)
        {
            yield return wait;
            SmellCheck();
        }
    }
    private void SmellCheck()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius);
        targetRef = null;
        int tempFoodCount = 0;
        if (hitColliders.Length > 0) return;
        foreach (Collider collider in hitColliders) {
            //Transform target = collider.transform;
            if (collider.CompareTag("food"))
            {
                tempFoodCount++;
            }
            //Vector3 directionToTarget = (target.position - transform.position).normalized;
            //float distanceToTarget = Vector3.Distance(transform.position, target.position);
            //if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, smellReductionMask))
           // {
            //    targetSmellIntensity = (1f - Math.Clamp(distanceToTarget / smellRadius, 0, 1));
            //    targetRef = collider.gameObject;
            //}
           // targetRef = collider.gameObject;
           // break;
        }
        HungerBehaviour.foodCount = tempFoodCount;
    }
}
