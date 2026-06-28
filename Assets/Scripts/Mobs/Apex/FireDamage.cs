using UnityEngine;

[RequireComponent (typeof(EntityHealthManager))]
public class FireDamage : MonoBehaviour
{
    public const float DAMAGE_PER_TIME_INTERVAL = 2.5f;
    public const float TIME_INTERVAL = 1.0f;

    private EntityHealthManager healthManager;

    private bool firePresent = true; // Whether we should check for fire damage, set to only true for now

    void Start()
    {
        healthManager = GetComponent<EntityHealthManager>();
        InvokeRepeating("CheckFireDamage", 0, TIME_INTERVAL);
    }

    /// <summary>
    /// If fire is present and game object is in fire, deal damage
    /// </summary>
    void CheckFireDamage()
    {
        if (firePresent && IsInFire())
        {
            // deal damage
            DamageContext context = new DamageContext();
            context.attacker = gameObject;
            context.victim = gameObject;
            context.amount = DAMAGE_PER_TIME_INTERVAL;
            context.xxtraContext = "Fire Damage";
        }
    }

    private bool IsInFire()
    {
        
    }

    private void SetFirePresent(bool firePresent)
    {
        this.firePresent = firePresent;
    }
}
