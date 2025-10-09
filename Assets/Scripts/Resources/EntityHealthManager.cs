using UnityEngine;

public class EntityHealthManager : MonoBehaviour, IHealth
{
    // default max health to 100
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth; // => used for read-only property

    public float CurrentHealth { get; private set; }

    [System.Serializable]
    public struct DamageContext
    {
        public GameObject Attacker; // who caused the damage
        public GameObject Victim;   // who is taking the damage
        public string ExtraContext; // any additional context, e.g., "Critical Hit", "Poisoned"
    }

    // possible events we may want?
    public System.Action<DamageContext> OnHealthChanged;
    public System.Action OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth; // start at full health

    }

    public void TakeDamage(float amount, GameObject attacker, string extra)
    {
        DamageContext attackContext = new DamageContext
        {
            Attacker = attacker,
            Victim = gameObject,
            ExtraContext = extra
        };

        if (CurrentHealth <= 0) return; // already dead, do nothing

        // reduce health but not below zero
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);

        OnHealthChanged?.Invoke(attackContext); // return info about the damage

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount, GameObject healer, string extra)
    {
        DamageContext healContext = new DamageContext
        {
            Attacker = healer,
            Victim = gameObject,
            ExtraContext = extra
        };

        if (CurrentHealth <= 0) return; // prob a design thing, maybe ability to revive dead creatures in the future?

        // increase health but not above max, maybe change in future to allow overheal?
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);

        OnHealthChanged?.Invoke(healContext);
    }

    public void Die()
    {
        // TODO: Add death logic here, for now just destroying game object
        Debug.Log($"{gameObject.name} has died.");
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

}
