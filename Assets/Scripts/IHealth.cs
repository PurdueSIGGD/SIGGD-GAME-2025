using UnityEngine;

public interface IHealth
{
    // required vars for any creature using health system
    float CurrentHealth { get; }
    float MaxHealth { get; }

    // required methods for any creature using health system
    void TakeDamage(float amount);
    void Heal(float amount);
    void Die();
}
