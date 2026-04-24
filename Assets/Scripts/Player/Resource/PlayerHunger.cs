using UnityEngine;
using UnityEngine.UI;

public class PlayerHunger : MonoBehaviour
{
    [SerializeField] bool inShipScene = false; // if in ship scene dont do hunger

    [SerializeField] float maxHunger = 100f;
    [SerializeField] float hungerDecayRate = 1f;
    [SerializeField] float hungerDamageInterval = 10f; // seconds between starvation damage
    [SerializeField] DamageContext hungerDamageContext;
    [SerializeField] GenericLingeringVignette hungerVignette;
    [SerializeField] Slider hungerSlider;
    public float MaxHunger => maxHunger;
    public float CurrentHunger
    {
        get => currentHunger;
        set
        {
            currentHunger = Mathf.Clamp(value, 0, maxHunger);
            UpdateSlider();
        }
    }

    private float currentHunger = -1;
    private float hungerDamageTimer;     // tracks time since last starvation tick
    private EntityHealthManager playerHealth;

    void Start()
    {
        if (currentHunger < 0) currentHunger = maxHunger;
        playerHealth = GetComponent<EntityHealthManager>();
        if (hungerSlider != null)
        {
            hungerSlider.minValue = 0f;
            hungerSlider.maxValue = maxHunger;
            hungerSlider.value = currentHunger;
        }
    }

    void Update()
    {
        if (inShipScene == false)
        {
            //hunger goes down and takes health when starving
            if (currentHunger > 0)
            {
                //currentHunger = Mathf.Max(currentHunger - hungerDecayRate * Time.deltaTime, 0);
                CurrentHunger -= hungerDecayRate * Time.deltaTime;
                hungerDamageTimer = 0f; // Reset timer if not starving
            }
            else
            {
                //player is starving stuff
                hungerDamageTimer += Time.deltaTime;

                if (hungerDamageTimer >= hungerDamageInterval)
                {
                    hungerDamageTimer = 0f; // Reset timer
                    playerHealth.TakeDamage(hungerDamageContext);
                    Debug.Log("Starving - Took 1 damage");
                }
            }
        }
        float hungerPercent = CurrentHunger / MaxHunger;
        float targetStrength = (1 - hungerPercent) * 1.5f;
        hungerVignette?.SetStrength(targetStrength);
    }

    private void UpdateSlider()
    {
        if (hungerSlider != null)
            hungerSlider.value = currentHunger;
    }

    public void UpdateHunger(float ammount)
    {
        CurrentHunger += ammount;
    }

    public void ResetHunger()
    {
        CurrentHunger = maxHunger;
    }
}



