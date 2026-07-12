using System.Collections;
using System.Transactions;
using UnityEngine;

[RequireComponent (typeof(EntityHealthManager))]
public class FireDamage : MonoBehaviour
{
    public float INITIAL_WAIT_BEFORE_DAMAGE = 0.5f; // Seconds in fire before entity takes damage
    public float DAMAGE_PER_TIME_INTERVAL = 2.5f;
    public float DAMAGE_TIME_INTERVAL = 1.0f; // after initial delay, this is interval between successive damages

    private EntityHealthManager healthManager;
    private Coroutine damageCoroutine;


    void Start()
    {
        healthManager = GetComponent<EntityHealthManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("FireVFX"))
        {
            StartFireDamage(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("FireVFX"))
        {
            StopFireDamage();
        }
    }

    /// <summary>
    /// Deals appropriate fire damage when given a health manager
    /// </summary>
    /// <param name="healthManager"></param>
    private IEnumerator DealFireDamage(GameObject fireGameObject)
    {
        // Initial delay

        Debug.Log("in fire waiting");
        yield return new WaitForSeconds(INITIAL_WAIT_BEFORE_DAMAGE);

        while (true)
        {
            // Deal damage

            DamageContext context = new DamageContext();
            context.attacker = fireGameObject;
            context.victim = this.gameObject;
            context.amount = DAMAGE_PER_TIME_INTERVAL;
            context.xxtraContext = "Fire Damage";

            healthManager.TakeDamage(context);

            // Delay

            yield return new WaitForSeconds(DAMAGE_TIME_INTERVAL);

        }
    }

    /// <summary>
    /// Start the fire damage coroutine
    /// </summary>
    /// <param name="fireGameObject"></param>
    private void StartFireDamage(GameObject fireGameObject)
    {
        if (damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(DealFireDamage(fireGameObject));
        }
    }

    private void StopFireDamage()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

}
