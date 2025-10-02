using UnityEngine;

public class EntityHealthManager : MonoBehaviour, IHealth
{
    // default max health to 100
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth; // => used for read-only property

    public float CurrentHealth { get; private set; }

    // possible events we may want?
    public System.Action<float> OnHealthChanged;
    public System.Action OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth; // start at full health
    }

    public void TakeDamage(float amount)
    {
        if (CurrentHealth <= 0) return; // already dead, do nothing

        // reduce health but not below zero
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);

        OnHealthChanged?.Invoke(CurrentHealth); // return current health on taking damage if needed (maybe for displaying health?)

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (CurrentHealth <= 0) return; // prob a design thing, maybe ability to revive dead creatures in the future?

        // increase health but not above max, maybe change in future to allow overheal?
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);

        OnHealthChanged?.Invoke(CurrentHealth);
    }

    public void Die()
    {
        // TODO: Add death logic here, for now just destroying game object
        Debug.Log($"{gameObject.name} has died.");
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
