using UnityEngine;
using UnityEngine.Rendering;

public class PlayerStats : MonoBehaviour
{
    public float maxHunger = 100f;
    public float currentHunger;
    public float hungerDecayRate = 1f;

[SerializeField] private Stat statManager;

    private float hungerDamageTimer = 0f;     // tracks time since last starvation tick
    [SerializeField] private float hungerDamageInterval = 10f; // seconds between starvation damage

    private EntityHealthManager playerHealth;



    void Start()
    {
        currentHunger = maxHunger;
        playerHealth = GetComponent<EntityHealthManager>();

        statManager.OnStatChanged.AddListener(OnStatChanged);

        
    }

    private void OnStatChanged(StatType stat, float value)
    {
        if (stat == StatType.maxHunger)
        {
            SetmaxHunger(value);
            
        }

        if (stat == StatType.hungerDecayRate)
        {
            SethungerDecayRate(value);
        }
    }

    public void SetmaxHunger(float newmaxHunger)
    {
        maxHunger = newmaxHunger;
    }
    public void SethungerDecayRate(float newhungerDecayRate)
    {
        hungerDecayRate = newhungerDecayRate;
    }

    void Update()
    {
        //Debug.Log(currentHunger);
        //Debug.Log(hungerDecayRate);
        //hunger goes down and takes health when starving
        if (currentHunger > 0)
        {
            currentHunger -= hungerDecayRate * Time.deltaTime;
            hungerDamageTimer = 0f; // Reset timer if not starving

         
        } 


        else 
        {
            //player is starving stuff
            //Debug.Log("Starving");
 
            hungerDamageTimer += Time.deltaTime;

            if (hungerDamageTimer >= hungerDamageInterval)
            {
                hungerDamageTimer = 0f; // Reset timer
                playerHealth.TakeDamage(1, gameObject, "Starving");
                Debug.LogWarning("Starving - Took 1 damage");
            }
        }
    }

    public void ChangeHunger(float amount)
    {
        currentHunger += amount;
    }
}



