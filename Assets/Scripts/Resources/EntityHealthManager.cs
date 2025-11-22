using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class EntityHealthManager : MonoBehaviour, IHealth
{
    // default max health to 100
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth; // => used for read-only property

    public float CurrentHealth { get; set; }

    // possible events we may want?
    public static Action<DamageContext> OnHealthChanged;
    public static Action<DamageContext> OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth; // start at full health
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

        // increase health but not above max, maybe change in future to allow overheal?
        CurrentHealth = Mathf.Min(CurrentHealth + healContext.amount, maxHealth);

        OnHealthChanged?.Invoke(healContext);
    }

    public void Die(DamageContext damageContext)
    {  
        // disabling player death for now, remove after respawn is implemented
        if (gameObject == PlayerID.Instance.gameObject)
        {
            SceneManager.LoadScene("Main Menu");
            return;
        }


        // TODO: Add death logic here, for now just destroying game object
        Debug.Log($"{gameObject.name} has died.");
        OnDeath?.Invoke(damageContext);
        Destroy(gameObject);
    }
    

}
