using UnityEngine;

public interface IHealth
{
    // required vars for any creature using health system
    float CurrentHealth { get; }
    float MaxHealth { get; }

    // required methods for any creature using health system
    void TakeDamage(float amount, GameObject attacker, string extra);
    void Heal(float amount, GameObject healer, string extra);
    void Die();
}
