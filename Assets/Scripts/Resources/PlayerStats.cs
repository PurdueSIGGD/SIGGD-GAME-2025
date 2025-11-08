using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float maxHunger = 100f;
    public float currentHunger;
    public float hungerDecayRate = 1f;

    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDecayRate = 2f;
    public float staminaRegenRate = 1f;

    [SerializeField] private Stat statManager;

    private float hungerDamageTimer = 0f;     // tracks time since last starvation tick
    [SerializeField] private float hungerDamageInterval = 10f; // seconds between starvation damage

    private EntityHealthManager playerHealth;



    void Start()
    {
        currentHunger = maxHunger;
        playerHealth = GetComponent<EntityHealthManager>();

        currentStamina = maxStamina;

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

        if (stat == StatType.maxStamina)
        {
            SetmaxStamina(value);

        }

        if (stat == StatType.staminaDecayRate)
        {
            SetstaminaDecayRate(value);
        }

        if (stat == StatType.staminaRegenRate)
        {
            SetstaminaRegenRate(value);
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

    public void SetmaxStamina(float newmaxStamina)
    {
        maxStamina = newmaxStamina;
    }
    public void SetstaminaDecayRate(float newstaminaDecayRate)
    {
        staminaDecayRate = newstaminaDecayRate;
    }
    public void SetstaminaRegenRate(float newstaminaDecayRate)
    {
        staminaDecayRate = newstaminaDecayRate;
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


        // TODO make different decay rates for each type of activity
        // TODO make a way to tell if the player is climbing/sprinting/etc

        // stamina decays while exerting effort (climb, sprint; jump triggers once?)

        //if (currentStamina<=0) // if the player runs out of stamina
        //{
        //    if (climbing == true)
        //    {
        //        // fall off wall
        //    }
        //    else if (sprinting == true)
        //    {
        //        // stop sprinting
        //    }
        //    // jumping should trigger once, don't need to keep checking
        //}
        //else if (climbing == true)
        //{
        //    currentStamina -= staminaDecayRate * Time.deltaTime;
        //}
        //else if (sprinting == true)
        //{
        //    currentStamina -= staminaDecayRate * Time.deltaTime;
        //}
        //// stamina regens while not exerting effort
        //else // if the player isn't doing effort (has stamina & isn't climbing or sprinting)
        //{
        //    currentStamina += staminaRegenRate * Time.deltaTime;
        //}

    }

    public void ChangeHunger(float amount)
    {
        currentHunger += amount;
    }

    //for when player jumps or does a climb burst; steady decreases are handled in update
    public void ChangeStamina(float amount)
    {
        currentStamina += amount;
    }
}



