using NUnit.Framework.Interfaces;
using UnityEngine;


public class LungeAttackBehaviour : MonoBehaviour
{
    public float attackInterval;
    public float telegraph;
    public float beginningAttackCooldown;
    public float attackOffset;
    private EnemyAnimator animatorController;
    private Rigidbody rb;

    Animator animator;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        beginningAttackCooldown = 100f;
        animatorController = GetComponent<EnemyAnimator>();

    }

    // Update is called once per frame
    void Update()
    {
    }
    private void LungeMovement()
    {
        rb.AddForce(transform.forward, ForceMode.Impulse);
        animatorController.PlayAttack();
    }
}
