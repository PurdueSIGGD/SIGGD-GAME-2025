using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class EnemyAnimator : MonoBehaviour
{
    //  is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private LayerMask mobLayer;
    private Animator animator;
    private BoxCollider collider;
    private Vector3 boxHalfExtents;
    private Vector3 boxCenter;
    void Awake()

    {
        //playerLayer = LayerMask.GetMask("Player");
        mobLayer = LayerMask.GetMask("Mob");
        animator = GetComponent<Animator>();
        collider = GetComponentInChildren<BoxCollider>();
    }
    private void Start()
    {
        boxHalfExtents = collider.size * 0.5f;
        boxCenter = collider.transform.TransformPoint(collider.center);
    }

    public void PlayAttack() => animator.SetTrigger("Attack");
    /*
    public void AttackHitboxCheck()
    {
        draw = true;
        Collider[] results = new Collider[3];
        int hits = Physics.OverlapBoxNonAlloc(boxCenter, boxHalfExtents, results, Quaternion.identity, mobLayer);
        for (int i = 0; i < hits; i++)
        {
            EntityHealthManager hm = results[i].GetComponent<EntityHealthManager>();

            if (hm != null)
            {
                hm.TakeDamage(30, this.gameObject, "Hyena has damaged mob");
            }
        }
    }
    */
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

    void EnableAttack()
    {
        Debug.Log("attack_enabled");
        //collider.gameObject.SetActive(true);
        collider.enabled = true;
    }
    void DisableAttack()
    {
        Debug.Log("attack_disabled");
        //collider.gameObject.SetActive(false);

        collider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        EntityHealthManager hm = other.GetComponent<EntityHealthManager>();
        if (hm != null)
        {
            hm.TakeDamage(4, this.gameObject, "hyena has lunge attacked");
        }
    }
}
