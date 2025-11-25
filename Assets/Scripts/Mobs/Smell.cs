using SIGGD.Mobs;
using SIGGD.Goap.Capabilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;

public class Smell : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float smellRadius;
    public LayerMask targetMask;
    public LayerMask smellReductionMask;
    public float targetSmellIntensity;
    public GameObject targetRef;
    private HungerBehaviour HungerBehaviour;
    private PreyBehaviour PreyBehaviour;
    private bool isPrey;
    private bool isPredator;
    private Vector3 sumPreyPositions;
    private Vector3 sumPredatorPositions;
    public Vector3 positionTest;

    private Action SmellCheck;
    void Awake()
    {
        HungerBehaviour = GetComponent<HungerBehaviour>();
        PreyBehaviour = GetComponent<PreyBehaviour>();
    }
    void Start()
    {
        positionTest = Vector3.zero;
        if (gameObject.CompareTag("Prey")) isPrey = true;
        if (isPrey && isPredator)
        {
            SmellCheck = SmellCheckPredatorPrey;
        }
        else if (isPrey)
        {
            SmellCheck = SmellCheckPrey;
        } else
        {
            SmellCheck = SmellCheckPredator;
        }
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
    //  if (collider.CompareTag("mobs"))
    //   {
    //    tempFoodCount++;
    // }
    //Vector3 directionToTarget = (target.position - transform.position).normalized;
    //float distanceToTarget = Vector3.Distance(transform.position, target.position);
    //if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, smellReductionMask))
    // {
    //    targetSmellIntensity = (1f - Math.Clamp(distanceToTarget / smellRadius, 0, 1));
    //    targetRef = collider.gameObject;
    //}
    // targetRef = collider.gameObject;
    // break;
    private void SmellCheckPredatorPrey()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius);
        targetRef = null;
        int tempFoodCount = 0;
        int tempPreyCount = 0;
        int tempPredatorCount = 0;
        Vector3 tempSumPredatorPositions = Vector3.zero;
        Vector3 tempSumPreyPositions = Vector3.zero;
        if (hitColliders.Length < 1) return;
        foreach (Collider collider in hitColliders)
        {
            //Transform target = collider.transform;
            if (collider.CompareTag("Food"))
            {
                tempFoodCount++;
            }
            tempSumPredatorPositions += collider.gameObject.transform.position;
            tempSumPreyPositions += collider.gameObject.transform.position;
        }
        HungerBehaviour.foodCount = tempFoodCount;
        sumPreyPositions = (tempPreyCount != 0) ? tempSumPreyPositions / tempPreyCount : Vector3.zero;
        sumPredatorPositions = (tempPredatorCount != 0) ? tempSumPredatorPositions / tempPredatorCount : Vector3.zero;
    }
    private void SmellCheckPrey()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius);
        targetRef = null;
        int tempFoodCount = 0;
        int tempPredatorCount = 0;
        Vector3 tempSumPredatorPositions = Vector3.zero;
        if (hitColliders.Length < 1) return;
        foreach (Collider collider in hitColliders)
        {
            //Transform target = collider.transform;
            if (collider.CompareTag("Food"))
            {
                tempFoodCount++;
            }
            if (collider.CompareTag("Predator"))
            {
                tempSumPredatorPositions += collider.gameObject.transform.position;
                tempPredatorCount++;
            }
        }
        HungerBehaviour.foodCount = tempFoodCount;
        sumPredatorPositions = (tempPredatorCount != 0) ? (tempSumPredatorPositions / tempPredatorCount) : Vector3.zero;
        PreyBehaviour.predatorCount = tempPredatorCount; 
    }
    private void SmellCheckPredator()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smellRadius);
        targetRef = null;
        int tempFoodCount = 0;
        int tempPreyCount = 0;
        Vector3 tempSumPreyPositions = Vector3.zero;
        if (hitColliders.Length < 1) return;
        foreach (Collider collider in hitColliders)
        {
            //Transform target = collider.transform;
            if (collider.CompareTag("Food"))
            {
                tempFoodCount++;
            }
            tempSumPreyPositions += collider.gameObject.transform.position;
        }
        HungerBehaviour.foodCount = tempFoodCount;
        sumPreyPositions = (tempPreyCount != 0) ? tempSumPreyPositions / tempPreyCount : Vector3.zero;
    }
    public Vector3 GetSumPredatorPositions()
    {
        return sumPredatorPositions;
    }
    public Vector3 GetSumPreyPositions()
    {
        return sumPreyPositions;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(positionTest, 2f);
    }
}
