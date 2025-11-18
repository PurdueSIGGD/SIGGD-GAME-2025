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

    private float currentHunger = -1f;
    private float hungerDamageTimer = 0f;     // tracks time since last starvation tick
    private EntityHealthManager playerHealth;

    void Start()
    {
        if (currentHunger < 0)
        {
            currentHunger = maxHunger;
        }
        playerHealth = GetComponent<EntityHealthManager>();
    }

    void Update()
    {
        //hunger goes down and takes health when starving
        if (currentHunger > 0)
        {
            currentHunger -= hungerDecayRate * Time.deltaTime;
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
}



