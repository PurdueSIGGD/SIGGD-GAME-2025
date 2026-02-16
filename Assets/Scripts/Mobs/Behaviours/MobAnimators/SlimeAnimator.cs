using UnityEngine;
using UnityEngine.AI;

public class SlimeAnimator : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private Animator animator;
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        float speedAnim = navAgent.velocity.magnitude / Mathf.Max(navAgent.speed, 0.01f);
        animator.SetFloat("Speed", speedAnim);
    }
}
