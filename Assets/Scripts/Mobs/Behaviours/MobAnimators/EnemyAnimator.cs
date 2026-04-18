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
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask mobLayer;
    [SerializeField] private Animator animator;
    [SerializeField] Collider damageCollider;
    private Vector3 boxHalfExtents;
    private Vector3 boxCenter;
    [SerializeField] private DamageContext damageContext;

    [SerializeField]
    private Transform lookTargetTransform;
    [SerializeField]
    private Transform forwardTransform;
    [SerializeField]
    private Transform aimTransform;

    [SerializeField]
    private float maxLookAngle = 90f;

    public float targetT = 0f;
    Vector3 vel;
    public float t;

    private readonly string hyenaDamagedSound = "HyenaOnDamage";
    private readonly string hyenaDamagePlayerSound = "HyenaOnDamagePlayer";
    private readonly string hyenaDeathSound = "HyenaOnSlain";

    private void Start()
    {
        boxHalfExtents = damageCollider.bounds.size * 0.5f;
        boxCenter = damageCollider.transform.TransformPoint(damageCollider.bounds.center);
        if (aimTransform != null || forwardTransform != null)
            aimTransform.position = forwardTransform.position;
    }

    private void OnEnable()
    {
        EntityHealthManager.OnHealthChanged += HandleDamage;
        EntityHealthManager.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        EntityHealthManager.OnHealthChanged -= HandleDamage;
        EntityHealthManager.OnDeath -= HandleDeath;
    }

    void LateUpdate()
    {
        if (!aimTransform || !forwardTransform) return;

        float currentTargetT = targetT;

        if (lookTargetTransform != null)
        {
            Vector3 dirToTarget = (lookTargetTransform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle > maxLookAngle)
            {
                currentTargetT = 0f;
            }
        }

        t = Mathf.MoveTowards(t, currentTargetT, 6f * Time.deltaTime);

        Vector3 forwardPos = forwardTransform.position;
        Vector3 targetPos = (lookTargetTransform != null) ? lookTargetTransform.position : forwardPos;

        Vector3 desired = Vector3.Lerp(forwardPos, targetPos, t);

        aimTransform.position = Vector3.SmoothDamp(aimTransform.position, desired, ref vel, 0.08f, Mathf.Infinity, Time.deltaTime);
    }

    private void HandleDamage(DamageContext ctx)
    {
        if (ctx.amount > 0)
        {
            Debug.Log($"handling damage... {ctx.attacker} {ctx.attacker == gameObject} {ctx.victim} {ctx.victim == PlayerID.Instance.gameObject} {ctx.amount}");
        }
        if (ctx.victim == gameObject && ctx.amount > 0)
        {
            Debug.Log("playing hyena damage sound");
            AudioManager.Instance.PlayOneShotNoAsync(hyenaDamagedSound, transform.position);

        }
        else if (ctx.attacker == gameObject && ctx.victim == PlayerID.Instance.gameObject && ctx.amount > 0)
        {
            Debug.Log("playing player taking damage from hyena sound");
            AudioManager.Instance.PlayOneShotNoAsync(hyenaDamagePlayerSound, PlayerID.Instance.gameObject.transform.position);

        }
    }

    private void HandleDeath(DamageContext ctx)
    {
        if (ctx.victim == gameObject)
        {
            AudioManager.Instance.PlayOneShotNoAsync(hyenaDeathSound, transform.position);
        }
    }

    public void SetLook(bool look)
    {
        targetT = look ? 1f : 0f;
    }
    public void PlayAttack()
    {
        animator.SetBool("Attack", true);
    }
    public void EndAttack()
    {
        animator.SetBool("Attack", false);
        damageCollider.enabled = false;
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
        if (damageCollider == null) return;
        if (damageCollider.enabled)
        {
            Gizmos.color = Color.red;
        } else
        {
            Gizmos.color = Color.clear;
        }
        Gizmos.matrix = damageCollider.transform.localToWorldMatrix;
        Gizmos.DrawCube(damageCollider.bounds.center, boxHalfExtents * 2);
    }

    public void SetLungeModel()
    {
        PlayAttack();
        damageCollider.enabled = true;
    }

    void EnableAttack()
    {
        Debug.Log("attack_enabled");

        damageCollider.enabled = true;
    }
    void DisableAttack()
    {
        Debug.Log("attack_disabled");

        damageCollider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        EntityHealthManager hm = other.GetComponent<EntityHealthManager>();
        if (other.CompareTag("Player"))
        {
            Debug.Log("hit");
        }
        if (hm != null)
        {
            if (other.CompareTag("Predator")) return;
            damageContext.attacker = gameObject;
            damageContext.victim = hm.gameObject;
            hm.TakeDamage(damageContext);
        }
    }
}
