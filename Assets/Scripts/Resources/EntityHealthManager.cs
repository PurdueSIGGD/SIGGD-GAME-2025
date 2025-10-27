using UnityEngine;
using System;
using UnityEngine.UIElements;

public class EntityHealthManager : MonoBehaviour, IHealth
{
    // default max health to 100
    public float MaxHealth { get; private set; } = 100f;

    public float CurrentHealth { get; private set; }


    private Stat statManager;


    [System.Serializable]
    public struct DamageContext
    {
        public GameObject Attacker; // who caused the damage
        public GameObject Victim;   // who is taking the damage
        public string ExtraContext; // any additional context, e.g., "Critical Hit", "Poisoned"
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        MaxHealth = newMaxHealth;
        // Ensure current health does not exceed new max health
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
    }

    // possible events we may want?
    public Action<DamageContext> OnHealthChanged;
    public Action OnDeath;

    private void Awake()
    {
        CurrentHealth = MaxHealth; // start at full health
        if (TryGetComponent<Stat>(out var statComp))
        {
            statManager = statComp;
            SetMaxHealth(statManager.GetStat(StatType.maxHealth));
            CurrentHealth = MaxHealth; // reset current health to new max
        }
    }

    private void Update()
    {
        
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

        Debug.Log($"{gameObject.name} took {amount} damage from {attacker.name}. Current Health: {CurrentHealth}/{MaxHealth}");

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
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);

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
