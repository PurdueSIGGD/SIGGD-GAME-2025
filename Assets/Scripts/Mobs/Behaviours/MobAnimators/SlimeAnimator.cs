using UnityEngine;
using UnityEngine.AI;

public class SlimeAnimator : MonoBehaviour
{
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Animator animator;

    void Update()
    {
        float speedAnim = navAgent.velocity.magnitude / Mathf.Max(navAgent.speed, 0.01f);
        animator.SetFloat("Speed", speedAnim);
    }
}
