using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.LowLevel;
using Utility;

public class EnemyAnimator : MonoBehaviour
{
    // is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private LayerMask mobLayer;
    private Animator animator;
    private BoxCollider collider;
    private Vector3 boxHalfExtents;
    private Vector3 boxCenter;
    [SerializeField] private DamageContext damageContext;

    [SerializeField]
    private Transform lookTargetTransform;
    [SerializeField]
    private Transform forwardTransform;
    [SerializeField]
    private Transform aimTransform;

    public float targetT = 0f;
    Vector3 vel;
    public float t;
    void Awake()

    {
        //playerLayer = LayerMask.GetMask("Player");
        mobLayer = LayerMask.GetMask("Mob");
        animator = GetComponentInChildren<Animator>();
        collider = GetComponentInChildren<BoxCollider>();
        aimTransform.position = forwardTransform.position;
    }
    private void Start()
    {
        boxHalfExtents = collider.size * 0.5f;
        boxCenter = collider.transform.TransformPoint(collider.center);
    }
    void LateUpdate()
    {
        if (!aimTransform || !forwardTransform) return;

        t = Mathf.MoveTowards(t, targetT, 6f * Time.deltaTime);

        Vector3 forwardPos = forwardTransform.position;
        Vector3 targetPos = (lookTargetTransform != null) ? lookTargetTransform.position : forwardPos;

        Vector3 desired = Vector3.Lerp(forwardPos, targetPos, t);

        aimTransform.position = Vector3.SmoothDamp(aimTransform.position, desired, ref vel, 0.08f, Mathf.Infinity, Time.deltaTime);
    }
    public void SetLook(bool look)
    {
        targetT = look ? 1f : 0f;
    }
    public void PlayAttack() => animator.SetBool("Attack", true);
    public void EndAttack()
    {
        animator.SetBool("Attack", false);
        collider.enabled = false;
    }
    public AnimatorStateInfo getAnimStateInfo()
    {
        return animator.GetCurrentAnimatorStateInfo(0);
    }
    public float getAnimLength()
    {
        return getAnimStateInfo().length;
    }
    public void SetLookTarget(Transform target)
    {
        lookTargetTransform = target;
    }
    private void OnDrawGizmos()
    {
        if (collider == null) return;
        if (collider.enabled)
        {
            Gizmos.color = Color.red;
        } else
        {
            Gizmos.color = Color.clear;
        }
        Gizmos.matrix = collider.transform.localToWorldMatrix;
        Gizmos.DrawCube(collider.center, boxHalfExtents * 2);
    }

    public void SetLungeModel()
    {
        PlayAttack();
    }

    void EnableAttack()
    {
        Debug.Log("attack_enabled");

        collider.enabled = true;
    }
    void DisableAttack()
    {
        Debug.Log("attack_disabled");

        collider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        EntityHealthManager hm = other.GetComponent<EntityHealthManager>();
        if (hm != null)
        {
            if (other.CompareTag("Predator")) return;
            damageContext.victim = hm.gameObject;
            hm.TakeDamage(damageContext);
        }
    }
}
