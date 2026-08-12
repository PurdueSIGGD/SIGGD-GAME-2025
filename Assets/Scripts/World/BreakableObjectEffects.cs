using UnityEngine;
using FMODUnity;

public class BreakableObjectEffects : MonoBehaviour
{
    [SerializeField] private GameObject breakParticles;
    [SerializeField] private EventReference breakSound;

    private void OnEnable()
    {
        EntityHealthManager.OnDeath += OnEntityDeath;
    }

    private void OnDisable()
    {
        EntityHealthManager.OnDeath -= OnEntityDeath;
    }

    private void OnEntityDeath(DamageContext damageContext)
    {
        // Ignore deaths that aren't this bush
        if (damageContext.victim != gameObject)
            return;

        if (breakParticles != null)
            Instantiate(breakParticles, transform.position, Quaternion.identity);

        RuntimeManager.PlayOneShot(breakSound, transform.position);
    }
}
