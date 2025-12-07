using UnityEngine;
using static Effects;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] float damagePulseIntensity;
    [SerializeField] private float duration = 1f;

    void OnEnable()
    {
        EntityHealthManager.OnHealthChanged += TriggerOnDamagePulse;
    }

    void OnDisable()
    {
        EntityHealthManager.OnHealthChanged -= TriggerOnDamagePulse;
    }

    private void TriggerOnDamagePulse(DamageContext context)
    {
        if (context.victim != PlayerID.Instance.gameObject) return;
        if (context.amount <= 0) return;
        SpecialEffects.VignetteEffect(damagePulseIntensity, duration);
    }
}
