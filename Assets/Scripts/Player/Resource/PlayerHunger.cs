using UnityEngine;

public class PlayerHunger : MonoBehaviour
{
    [SerializeField] float maxHunger = 100f;
    [SerializeField] float hungerDecayRate = 1f;
    [SerializeField] float hungerDamageInterval = 10f; // seconds between starvation damage
    [SerializeField] DamageContext hungerDamageContext;

    public float MaxHunger => maxHunger;
    public float CurrentHunger
    {
        get => currentHunger;
        set => currentHunger = Mathf.Clamp(value, 0, maxHunger);
    }

    private float currentHunger = -1;
    private float hungerDamageTimer;     // tracks time since last starvation tick
    private EntityHealthManager playerHealth;

    void Start()
    {
        if (currentHunger < 0) currentHunger = maxHunger;
        playerHealth = GetComponent<EntityHealthManager>();
    }

    void Update()
    {
        //hunger goes down and takes health when starving
        if (currentHunger > 0)
        {
            currentHunger = Mathf.Max(currentHunger - hungerDecayRate * Time.deltaTime, 0);
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

    public void UpdateHunger(float ammount)
    {
        currentHunger += ammount;
    }

    public void ResetHunger()
    {
        currentHunger = maxHunger;
    }
}



