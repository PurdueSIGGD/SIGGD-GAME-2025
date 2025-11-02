using UnityEngine;

public interface IHealth
{


    // required vars for any creature using health system
    float CurrentHealth { get; }
    float MaxHealth { get; }

    // required methods for any creature using health system
    void TakeDamage(DamageContext damageContext)
    {

    }
    void Heal(DamageContext damageContext);
    void Die(DamageContext damageContext);
}

[System.Serializable]
public struct DamageContext
{
    public GameObject attacker; // who caused the damage
    public GameObject victim;   // who is taking the damage
    public float amount; // how much damage
    public string xxtraContext; // any additional context, e.g., "Critical Hit", "Poisoned"
}
