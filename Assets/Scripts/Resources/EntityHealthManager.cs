using Sirenix.OdinInspector.Editor;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class EntityHealthManager : MonoBehaviour, IHealth
{
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }


    [SerializeField] private Stat statManager;

    [System.Serializable]
    public struct DamageContext
    {
        public GameObject Attacker; // who caused the damage
        public GameObject Victim;   // who is taking the damage
        public string ExtraContext; // any additional context
    }

    // possible events we may want?
    public Action<DamageContext> OnHealthChanged;
    public Action OnDeath;


    private void Start()
    {
        statManager.OnStatChanged.AddListener(OnStatChanged);

        MaxHealth = statManager.GetStat(StatType.maxHealth);
        CurrentHealth = MaxHealth;
    }


    // this makes sure that if max health changes via stats, we update it here
    private void OnStatChanged(StatType stat, float value)
    {
        if (stat == StatType.maxHealth)
        {
            SetMaxHealth(value);
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners when destroyed
        statManager.OnStatChanged.RemoveListener(OnStatChanged);
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        MaxHealth = newMaxHealth;
        // Ensure current health does not exceed new max health
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
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

        // increase health but not above max
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
