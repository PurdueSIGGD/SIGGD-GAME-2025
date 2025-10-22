using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    //  is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private LayerMask mobLayer;
    private Animator animator;
    public bool draw = false;
    void Awake()

    {
        //playerLayer = LayerMask.GetMask("Player");
        mobLayer = LayerMask.GetMask("Mob");
        animator = GetComponent<Animator>();
    }

    public void PlayAttack() => animator.SetTrigger("Attack");
    public void AttackHitboxCheck()
    {
        draw = true;
        Collider[] results = new Collider[3];
        int hits = Physics.OverlapBoxNonAlloc(gameObject.transform.position + transform.forward * 2, new Vector3(1,1,1), results, Quaternion.identity, mobLayer);
        for (int i = 0; i < hits; i++)
        {
            EntityHealthManager hm = results[i].GetComponent<EntityHealthManager>();

            if (hm != null)
            {
                hm.TakeDamage(30, this.gameObject, "Hyena has damaged mob");
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (draw)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(gameObject.transform.position + transform.forward * 2, new Vector3(1, 1, 1));
        }
    }
}
