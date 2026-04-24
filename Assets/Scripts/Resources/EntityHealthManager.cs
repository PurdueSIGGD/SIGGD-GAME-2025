using UnityEngine;
using System;

public class EntityHealthManager : StatProvider, IHealth
{
    // default max health to 100
    [SerializeField] public Stat maxHealth = new(100f);
    public float MaxHealth => maxHealth.Value; // => used for read-only property

    public float CurrentHealth { get; set; }

    // possible events we may want?
    public static Action<DamageContext> OnHealthChanged;
    public static Action<DamageContext> OnDeath;

    private static string playerDeathSound = "maledeath";

    void Start()
    {
        if (CurrentHealth == 0) CurrentHealth = maxHealth.Value; // start at full health
    }

    public void TakeDamage(DamageContext damageContext)
    {
        if (CurrentHealth <= 0) return; // already dead, do nothing

        // reduce health but not below zero
        CurrentHealth = Mathf.Max(CurrentHealth - damageContext.amount, 0);

        OnHealthChanged?.Invoke(damageContext); // return info about the damage

        Debug.Log($"{gameObject.name} took {damageContext.amount} damage from {damageContext.attacker}. Current Health: {CurrentHealth}/{maxHealth}");

        if (CurrentHealth <= 0)
        {
            Die(damageContext);
        }
    }

    public void Heal(DamageContext healContext)
    {
        if (CurrentHealth <= 0) return; // prob a design thing, maybe ability to revive dead creatures in the future?

        if (healContext.amount > 0)
        {
            Debug.LogWarning("Healing should be negative damage");
            return;
        }
        float healAmount = healContext.amount * -1; // healing is negative damage

        // increase health but not above max, maybe change in future to allow overheal?
        CurrentHealth = Mathf.Min(CurrentHealth + healAmount, maxHealth.value);

        OnHealthChanged?.Invoke(healContext);
    }

    public void Die(DamageContext damageContext)
    {
        // TODO: Add death logic here, for now just destroying game object
        Debug.Log($"{gameObject.name} has died.");
        OnDeath?.Invoke(damageContext);

        // player will be respawned, so do not destroy
        if (gameObject != PlayerID.Instance.gameObject)
        {
            // Attempt to change to peaceful if pursuer died

            if (GameStateManager.Instance) GameStateManager.Instance.attemptSetState(GameStateManager.GameState.PEACEFUL, gameObject);

            Destroy(gameObject);
        } else
        {
            // Change state of player to peaceful

            if (GameStateManager.Instance) GameStateManager.Instance.attemptSetState(GameStateManager.GameState.PEACEFUL, gameObject);
            AudioManager.Instance.PlayOneShotNoAsync(playerDeathSound, PlayerID.Instance.gameObject.transform.position);
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }
}
